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
        /// Body (optional): { "parentId": 99, "variants": [ { "variantTypeId": 5, "properties": {..} }, ... ] }
        /// When variants are supplied, the card is imported as one CardVariant per variant under the matched base card.
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

            var setCode = set.SetCode ?? string.Empty;

            if (request?.Variants is { Count: > 0 })
            {
                if (request.ParentId is null)
                    return BadRequest("A parent card is required to approve variants.");

                // Shared media for every variant of the preset (same art). The back media is only
                // created when the queued card actually has a back image.
                var (variantImageId, variantBackImageId) = CreateMediaForItem(item, BaseName(item.ExtractedData) ?? $"card_{id}");

                var models = new List<ImportModel>();
                foreach (var variant in request.Variants)
                {
                    var props = _queueService.BuildVariantProperties(variant.VariantTypeId, variant.Properties, setCode);
                    props.TryGetValue("Name", out var variantName);
                    if (string.IsNullOrWhiteSpace(variantName)) return BadRequest("A variant has no name.");

                    var attributes = props
                        .Where(kv => !kv.Key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);

                    // Only attach the back image to variants whose preset declares back_image_base64.
                    var useBackImage = variantBackImageId.HasValue && _queueService.VariantSupportsBackImage(variant.VariantTypeId);

                    models.Add(new ImportModel(null, variantName, set.Name, attributes)
                    {
                        ImageId = variantImageId,
                        BackImageId = useBackImage ? variantBackImageId : null,
                        ParentId = request.ParentId,
                        VariantTypeId = variant.VariantTypeId
                    });
                }

                _cardImporterService.Import(models);

                _queueRepository.UpdateSet(id, setId);
                _queueRepository.UpdateStatus(id, CardImportQueueStatus.Approved);
                return Ok();
            }

            // Base-card import: compute the read-only templated fields now that we know the set.
            Dictionary<string, string> data;
            try { data = JsonSerializer.Deserialize<Dictionary<string, string>>(item.ExtractedData) ?? []; }
            catch { data = []; }

            var withTemplates = _queueService.ApplyTemplates(data, setCode);

            // Build the card name (Name, optionally suffixed with Subname) and its attributes.
            withTemplates.TryGetValue("Name", out var name);
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("The card has no name.");
            if (withTemplates.TryGetValue("Subname", out var subname) && !string.IsNullOrWhiteSpace(subname))
                name = $"{name}, {subname}";

            var properties = withTemplates
                .Where(kv => !kv.Key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var (imageId, backImageId) = CreateMediaForItem(item, name);

            // Create the actual card content (same path as the manual ImportJsonFiles importer).
            _cardImporterService.Import(new[]
            {
                new ImportModel(null, name, set.Name, properties)
                {
                    ImageId = imageId,
                    BackImageId = backImageId
                }
            });

            // Persist the final computed data + set link, then mark approved.
            // The record (and its staged image) is kept for near-duplicate detection and
            // is removed later by CardImportQueueCleanupTask once it ages out.
            _queueRepository.UpdateExtractedData(id, JsonSerializer.Serialize(withTemplates));
            _queueRepository.UpdateSet(id, setId);
            _queueRepository.UpdateStatus(id, CardImportQueueStatus.Approved);
            return Ok();
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

            Dictionary<string, string> data;
            try { data = JsonSerializer.Deserialize<Dictionary<string, string>>(item.ExtractedData) ?? []; }
            catch { data = []; }

            data.TryGetValue("Name", out var cardName);
            if (data.TryGetValue("Subname", out var subname) && !string.IsNullOrWhiteSpace(subname))
                cardName = $"{cardName}, {subname}";

            var (baseId, matchedName) = _queueService.FindPotentialBaseCard(_siteAccessor.GetSiteId(), cardName ?? string.Empty);
            var status = baseId.HasValue ? CardImportQueueStatus.PotentialVariant : CardImportQueueStatus.Pending;
            _queueRepository.UpdateMatch(id, status, baseId);

            return Ok(new
            {
                Status = status,
                PotentialDuplicateId = baseId,
                MatchedCardName = matchedName
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
            if (item.PotentialDuplicateId.HasValue)
                matchedCardName = _cardService.Get(item.PotentialDuplicateId.Value)?.DisplayName;

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
                item.CreatedAt,
                item.SetId,
                ExtractedData = extractedData,
                TemplatedFields = templatedFields
            };
        }
    }

    public class ApproveRequest
    {
        /// <summary>Base card id to attach the variants to (required when Variants is non-empty).</summary>
        public int? ParentId { get; set; }
        public List<ApproveVariant> Variants { get; set; } = [];
    }

    public class ApproveVariant
    {
        public int VariantTypeId { get; set; }
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
