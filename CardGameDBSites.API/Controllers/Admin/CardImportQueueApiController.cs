using CardGameDBSites.API.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Controllers;
using Set = SkytearHorde.Entities.Generated.Set;

namespace CardGameDBSites.API.Controllers.Admin
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/cardimportqueue")]
    [JwtAuthorization]
    [ApiExplorerSettings(GroupName = "Admin")]
    public class CardImportQueueApiController : Controller
    {
        private readonly CardImportQueueRepository _queueRepository;
        private readonly CardImportQueueService _queueService;
        private readonly CardImporterService _cardImporterService;
        private readonly CardService _cardService;
        private readonly ISiteAccessor _siteAccessor;

        public CardImportQueueApiController(
            CardImportQueueRepository queueRepository,
            CardImportQueueService queueService,
            CardImporterService cardImporterService,
            CardService cardService,
            ISiteAccessor siteAccessor)
        {
            _queueRepository = queueRepository;
            _queueService = queueService;
            _cardImporterService = cardImporterService;
            _cardService = cardService;
            _siteAccessor = siteAccessor;
        }

        /// <summary>GET /umbraco/api/cardimportqueue/pending</summary>
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var siteId = _siteAccessor.GetSiteId();
            var items = _queueRepository.GetPending(siteId);
            return Ok(items.Select(MapToViewModel));
        }

        /// <summary>
        /// POST /umbraco/api/cardimportqueue/approve?id=123&amp;setId=456
        /// Body (optional): { "mode": "new|variant|reprint|update", "parentId": 99, "replaceImage": true,
        ///                    "variants": [ { "variantTypeId": 5, "properties": {..} }, ... ] }
        /// - new: creates a new base card in the set.
        /// - variant: creates one CardVariant per preset variant under the matched base card.
        /// - reprint: lists the set on the matched base card and gives its base printing the new art.
        /// - update: rewrites the matched base card with the queued data.
        /// When no mode is supplied it is inferred from the presence of variants (kept for older clients).
        /// </summary>
        [HttpPost("approve")]
        public IActionResult Approve(int id, int setId, [FromBody] ApproveRequest? request = null)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();

            var set = _cardService.GetAllSets().FirstOrDefault(s => s.Id == setId);
            if (set == null) return BadRequest("A valid set must be selected before approving.");

            request ??= new ApproveRequest();
            var mode = string.IsNullOrWhiteSpace(request.Mode)
                ? (request.Variants.Count > 0 ? ApproveMode.Variant : ApproveMode.New)
                : request.Mode.ToLowerInvariant();

            var failure = mode switch
            {
                ApproveMode.New => ApproveNewCard(item, set),
                ApproveMode.Variant => ApproveVariants(item, request, set),
                ApproveMode.Reprint => ApproveReprint(item, request, set),
                ApproveMode.Update => ApproveUpdate(item, request, set),
                _ => BadRequest($"Unknown approve mode '{mode}'.")
            };
            if (failure != null) return failure;

            // The record (and its staged image) is kept for near-duplicate detection and
            // is removed later by CardImportQueueCleanupTask once it ages out.
            _queueRepository.UpdateSet(id, setId);
            _queueRepository.UpdateStatus(id, CardImportQueueStatus.Approved);
            return Ok();
        }

        /// <summary>Creates a brand new base card in the set. Returns null on success.</summary>
        private IActionResult? ApproveNewCard(CardImportQueueDBModel item, Set set)
        {
            // Compute the read-only templated fields now that we know the set.
            var withTemplates = _queueService.ApplyTemplates(ReadExtractedData(item), set.SetCode ?? string.Empty);

            var name = BuildCardName(withTemplates);
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("The card has no name.");

            var (imageId, backImageId) = CreateMediaForItem(item, name);

            // Create the actual card content (same path as the manual ImportJsonFiles importer).
            _cardImporterService.Import(new[]
            {
                new ImportModel(null, name, set.Name, ToAttributes(withTemplates))
                {
                    ImageId = imageId,
                    BackImageId = backImageId
                }
            });

            _queueRepository.UpdateExtractedData(item.Id, JsonSerializer.Serialize(withTemplates));
            return null;
        }

        /// <summary>Creates the preset's variants under the matched base card. Returns null on success.</summary>
        private IActionResult? ApproveVariants(CardImportQueueDBModel item, ApproveRequest request, Set set)
        {
            if (request.ParentId is null)
                return BadRequest("A parent card is required to approve variants.");
            if (request.Variants.Count == 0)
                return BadRequest("No variants were supplied.");

            var (models, error) = BuildVariantModels(item, request, set);
            if (error != null) return BadRequest(error);

            _cardImporterService.Import(models);
            return null;
        }

        /// <summary>
        /// Approves the card as a reprint of the matched base card: the set is added to that card,
        /// which gives it a base printing (and its automatic variants) for the new set, and the queued
        /// art and fields are written to that base printing. Returns null on success.
        /// </summary>
        private IActionResult? ApproveReprint(CardImportQueueDBModel item, ApproveRequest request, Set set)
        {
            if (request.ParentId is null)
                return BadRequest("A parent card is required to approve a reprint.");
            if (_cardService.Get(request.ParentId.Value) is null)
                return BadRequest("The card being reprinted no longer exists.");

            _cardImporterService.AddSetToCard(request.ParentId.Value, set.Id);

            // The reprint always has its own art, so the base printing is imported even when the
            // chosen preset does not describe it (or no preset was chosen at all).
            if (!request.Variants.Any(it => it.VariantTypeId is null))
                request.Variants.Insert(0, new ApproveVariant());

            var (models, error) = BuildVariantModels(item, request, set);
            if (error != null) return BadRequest(error);

            _cardImporterService.Import(models);
            return null;
        }

        /// <summary>Rewrites the matched base card with the queued data. Returns null on success.</summary>
        private IActionResult? ApproveUpdate(CardImportQueueDBModel item, ApproveRequest request, Set set)
        {
            if (request.ParentId is null)
                return BadRequest("A card to update is required.");

            var existing = _cardService.Get(request.ParentId.Value);
            if (existing is null) return BadRequest("The card being updated no longer exists.");

            var withTemplates = _queueService.ApplyTemplates(ReadExtractedData(item), set.SetCode ?? string.Empty);
            var name = BuildCardName(withTemplates) ?? existing.DisplayName;

            // Start from the card's current attributes so fields the AI did not read are kept.
            var properties = existing.Attributes.ToDictionary(
                it => it.Value.GetAbility().IsMultiValue ? $"{it.Key}_multiple" : it.Key,
                it => it.Value.GetAbilityValue());
            foreach (var attribute in ToAttributes(withTemplates))
            {
                var key = properties.Keys.FirstOrDefault(it => IsSameAttribute(it, attribute.Key)) ?? attribute.Key;
                properties[key] = attribute.Value;
            }

            var (imageId, backImageId) = request.ReplaceImage ? CreateMediaForItem(item, name) : (null, null);

            _cardImporterService.Import(new[]
            {
                new ImportModel(request.ParentId, name, set.Name, properties)
                {
                    ImageId = imageId,
                    BackImageId = backImageId,
                    HideFromDecks = existing.HideFromDecks
                }
            });

            _queueRepository.UpdateExtractedData(item.Id, JsonSerializer.Serialize(withTemplates));
            return null;
        }

        /// <summary>
        /// Builds one import model per requested preset variant, all sharing the queued art. A variant
        /// without a variant type id is the set's base printing and is named after the card itself.
        /// Variants that already exist for the set are updated instead of duplicated.
        /// </summary>
        private (List<ImportModel> Models, string? Error) BuildVariantModels(CardImportQueueDBModel item, ApproveRequest request, Set set)
        {
            // Shared media for every variant of the preset (same art). The back media is only
            // created when the queued card actually has a back image.
            var (variantImageId, variantBackImageId) = CreateMediaForItem(item, BaseName(item.ExtractedData) ?? $"card_{item.Id}");
            var baseCardName = _cardService.Get(request.ParentId!.Value)?.DisplayName;

            var models = new List<ImportModel>();
            foreach (var variant in request.Variants)
            {
                var props = _queueService.BuildVariantProperties(variant.VariantTypeId, variant.Properties, set.SetCode ?? string.Empty);

                props.TryGetValue("Name", out var variantName);
                if (variant.VariantTypeId is null) variantName = baseCardName;
                if (string.IsNullOrWhiteSpace(variantName)) return ([], "A variant has no name.");

                // Only attach the back image to variants whose preset declares back_image_base64.
                var useBackImage = variantBackImageId.HasValue &&
                    (variant.VariantTypeId is null || _queueService.VariantSupportsBackImage(variant.VariantTypeId));

                models.Add(new ImportModel(
                    _cardImporterService.FindVariantId(request.ParentId.Value, set.Id, variant.VariantTypeId),
                    variantName,
                    set.Name,
                    ToAttributes(props))
                {
                    ImageId = variantImageId,
                    BackImageId = useBackImage ? variantBackImageId : null,
                    ParentId = request.ParentId,
                    VariantTypeId = variant.VariantTypeId
                });
            }

            return (models, null);
        }

        private static Dictionary<string, string> ReadExtractedData(CardImportQueueDBModel item)
        {
            try { return JsonSerializer.Deserialize<Dictionary<string, string>>(item.ExtractedData) ?? []; }
            catch { return []; }
        }

        /// <summary>The card name: Name, optionally suffixed with Subname.</summary>
        private static string? BuildCardName(IDictionary<string, string> data)
        {
            data.TryGetValue("Name", out var name);
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (data.TryGetValue("Subname", out var subname) && !string.IsNullOrWhiteSpace(subname))
                name = $"{name}, {subname}";

            return name;
        }

        /// <summary>The card fields that become attributes: everything filled in except the name.</summary>
        private static Dictionary<string, string> ToAttributes(IDictionary<string, string> data)
        {
            return data
                .Where(kv => !kv.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // The importer marks multi-value attributes with a "_multiple" suffix, so the same attribute
        // can be written with or without it depending on where the value came from.
        private static bool IsSameAttribute(string left, string right)
        {
            return left.Replace("_multiple", "").Equals(right.Replace("_multiple", ""), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>POST /umbraco/api/cardimportqueue/reject?id=123</summary>
        [HttpPost("reject")]
        public IActionResult Reject(int id)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();

            _queueRepository.UpdateStatus(id, CardImportQueueStatus.Rejected);
            return Ok();
        }

        /// <summary>POST /umbraco/api/cardimportqueue/updatedata?id=123  body: { "Name": "...", ... }</summary>
        [HttpPost("updatedata")]
        public IActionResult UpdateData(int id, [FromBody] Dictionary<string, string> extractedData)
        {
            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();

            _queueRepository.UpdateExtractedData(id, JsonSerializer.Serialize(extractedData));
            return Ok();
        }

        /// <summary>GET /umbraco/api/cardimportqueue/presets — variant presets for the variant picker.</summary>
        [HttpGet("presets")]
        public IActionResult GetPresets()
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            return Ok(_queueService.GetPresets());
        }

        /// <summary>
        /// POST /umbraco/api/cardimportqueue/rematch?id=123
        /// Re-runs the exact-name match against existing base cards using the item's current Name,
        /// flipping it to a potential variant when a base card is found (or back to pending otherwise).
        /// </summary>
        [HttpPost("rematch")]
        public IActionResult Rematch(int id)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();

            var cardName = BuildCardName(ReadExtractedData(item));

            var (baseId, matchedName) = _queueService.FindPotentialBaseCard(_siteAccessor.GetSiteId(), cardName ?? string.Empty);
            var status = baseId.HasValue ? CardImportQueueStatus.PotentialVariant : CardImportQueueStatus.Pending;
            _queueRepository.UpdateMatch(id, status, baseId);

            return Ok(new
            {
                Status = status,
                PotentialDuplicateId = baseId,
                MatchedCardName = matchedName,
                MatchedCardSetId = baseId.HasValue ? _cardService.Get(baseId.Value)?.SetId : null
            });
        }

        /// <summary>GET /umbraco/api/cardimportqueue/getimage?id=123</summary>
        [HttpGet("getimage")]
        public IActionResult GetImage(int id)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();
            if (string.IsNullOrWhiteSpace(item.ImagePath) || !System.IO.File.Exists(item.ImagePath))
                return NotFound();

            return ServeImageFile(item.ImagePath);
        }

        /// <summary>GET /umbraco/api/cardimportqueue/getbackimage?id=123</summary>
        [HttpGet("getbackimage")]
        public IActionResult GetBackImage(int id)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var item = _queueRepository.GetById(id);
            if (item == null) return NotFound();
            return ServeImageFile(item.BackImagePath);
        }

        private IActionResult ServeImageFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return NotFound();

            var ext = Path.GetExtension(path).ToLowerInvariant();
            var mimeType = ext == ".png" ? "image/png" : ext == ".webp" ? "image/webp" : "image/jpeg";
            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, mimeType);
        }

        /// <summary>
        /// POST /umbraco/api/cardimportqueue/submitmanual
        /// Manually submit one or more reveal images (base64) to the pipeline. Images are processed in
        /// order; a double-sided card (e.g. Leader) consumes the next image as its back side.
        /// Body: { "sourceUrl": "...", "images": [ { "imageBase64": "...", "mimeType": "image/png" } ] }
        /// </summary>
        [HttpPost("submitmanual")]
        public async Task<IActionResult> SubmitManual([FromBody] ManualSubmitRequest request)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var images = (request?.Images ?? [])
                .Where(img => !string.IsNullOrWhiteSpace(img.ImageBase64))
                .Select(img => (Base64: img.ImageBase64, MimeType: img.MimeType ?? "image/png"))
                .ToList();

            if (images.Count == 0)
                return BadRequest("No images provided.");

            var siteId = _siteAccessor.GetSiteId();
            var (total, succeeded, failed) = await _queueService.ProcessManualImagesAsync(images, siteId, request?.SourceUrl);

            return Ok(new ManualSubmitResult
            {
                Total = total,
                Succeeded = succeeded,
                Failed = failed
            });
        }

        /// <summary>Creates card image media from the item's staged image file(s), returning their ids.</summary>
        private (int? imageId, int? backImageId) CreateMediaForItem(CardImportQueueDBModel item, string name)
        {
            int? imageId = null;
            if (!string.IsNullOrWhiteSpace(item.ImagePath) && System.IO.File.Exists(item.ImagePath))
                imageId = _cardImporterService.CreateImageMediaFromBytes(name, System.IO.File.ReadAllBytes(item.ImagePath));

            int? backImageId = null;
            if (!string.IsNullOrWhiteSpace(item.BackImagePath) && System.IO.File.Exists(item.BackImagePath))
                backImageId = _cardImporterService.CreateImageMediaFromBytes($"{name}_back", System.IO.File.ReadAllBytes(item.BackImagePath));

            return (imageId, backImageId);
        }

        private static string? BaseName(string extractedDataJson)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(extractedDataJson);
                if (data != null && data.TryGetValue("Name", out var name) && !string.IsNullOrWhiteSpace(name))
                    return name;
            }
            catch { /* fall through */ }
            return null;
        }

        private object MapToViewModel(CardImportQueueDBModel item)
        {
            var extractedData = new Dictionary<string, string>();
            try { extractedData = JsonSerializer.Deserialize<Dictionary<string, string>>(item.ExtractedData) ?? []; }
            catch { /* return empty dict if data is malformed */ }

            extractedData.TryGetValue("Card Type", out var cardType);
            var templatedFields = _queueService.GetTemplatedFields(cardType);

            string? matchedCardName = null;
            int? matchedCardSetId = null;
            if (item.PotentialDuplicateId.HasValue)
            {
                var matchedCard = _cardService.Get(item.PotentialDuplicateId.Value);
                matchedCardName = matchedCard?.DisplayName;
                matchedCardSetId = matchedCard?.SetId;
            }

            var hasBackImage = !string.IsNullOrWhiteSpace(item.BackImagePath);

            return new
            {
                item.Id,
                item.SiteId,
                item.Status,
                item.SourceType,
                item.SourceUrl,
                ImageUrl = $"/api/cardimportqueue/getimage?id={item.Id}",
                HasBackImage = hasBackImage,
                BackImageUrl = hasBackImage ? $"/api/cardimportqueue/getbackimage?id={item.Id}" : null,
                item.PotentialDuplicateId,
                MatchedCardName = matchedCardName,
                MatchedCardSetId = matchedCardSetId,
                item.CreatedAt,
                item.SetId,
                ExtractedData = extractedData,
                TemplatedFields = templatedFields
            };
        }
    }

    public static class ApproveMode
    {
        public const string New = "new";
        public const string Variant = "variant";
        public const string Reprint = "reprint";
        public const string Update = "update";
    }

    public class ApproveRequest
    {
        /// <summary>One of <see cref="ApproveMode"/>; inferred from Variants when omitted.</summary>
        public string? Mode { get; set; }
        /// <summary>The matched base card: required for the variant, reprint and update modes.</summary>
        public int? ParentId { get; set; }
        public List<ApproveVariant> Variants { get; set; } = [];
        /// <summary>Update mode: whether the card's art is replaced by the queued image.</summary>
        public bool ReplaceImage { get; set; } = true;
    }

    public class ApproveVariant
    {
        /// <summary>Null for the set's base printing (a card variant without a variant type).</summary>
        public int? VariantTypeId { get; set; }
        /// <summary>Editable (non-templated) field values; templated fields are computed server-side.</summary>
        public Dictionary<string, string> Properties { get; set; } = [];
    }

    public class ManualSubmitRequest
    {
        /// <summary>Optional source URL applied to every image that does not specify its own.</summary>
        public string? SourceUrl { get; set; }
        public List<ManualSubmitImage> Images { get; set; } = [];
    }

    public class ManualSubmitImage
    {
        public string ImageBase64 { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public string? SourceUrl { get; set; }
    }

    public class ManualSubmitResult
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<ManualSubmitImageResult> Results { get; set; } = [];
    }

    public class ManualSubmitImageResult
    {
        public int Index { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
