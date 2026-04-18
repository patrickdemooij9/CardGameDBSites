using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPoco;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Integrations.TcgPlayer;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Database;
using System.Net.Http.Json;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using static SkytearHorde.Business.Helpers.BlockListCreatorHelper;

namespace SkytearHorde.Business.Controllers
{
    [PluginController("management")]
    public class ManagementController : UmbracoAuthorizedApiController
    {
        private readonly CardService _cardService;
        private readonly IContentService _contentService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly ISiteService _siteService;
        private readonly IScopeProvider _scopeProvider;
        private readonly IMediaService _mediaService;
        private readonly MediaFileManager _mediaFileManager;
        private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly CardGameSettingsConfig _cardGameSettingsConfig;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(CardService cardService, IContentService contentService, ISiteAccessor siteAccessor, ISiteService siteService, IScopeProvider scopeProvider,
            IMediaService mediaService, MediaFileManager mediaFileManager, MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IOptions<CardGameSettingsConfig> cardGameSettingsConfigOption, ILogger<ManagementController> logger)
        {
            _cardService = cardService;
            _contentService = contentService;
            _siteAccessor = siteAccessor;
            _siteService = siteService;
            _scopeProvider = scopeProvider;
            _mediaService = mediaService;
            _mediaFileManager = mediaFileManager;
            _mediaUrlGenerators = mediaUrlGenerators;
            _shortStringHelper = shortStringHelper;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _cardGameSettingsConfig = cardGameSettingsConfigOption.Value;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult DeleteVariants(int setId, int siteId, int variantTypeId)
        {
            _siteAccessor.SetSiteId(siteId);

            var cards = _cardService.GetAllBySet(setId, true).Where(it => it.VariantTypeId == variantTypeId);
            var deletedItems = 0;
            foreach (var card in cards)
            {
                var umbracoContent = _contentService.GetById(card.VariantId);
                if (umbracoContent != null)
                {
                    _contentService.Delete(umbracoContent);
                    deletedItems++;
                }
            }
            return Ok($"Deleted {deletedItems} variants");
        }

        [HttpPost]
        public IActionResult SetVariantIdsCheck(int setId, int siteId, int variantTypeId, int startingNumber, string attributeId, int? parentTypeId = null)
        {
            _siteAccessor.SetSiteId(siteId);
            var resultMessage = $"Set variant ids for set {setId} and variant type {variantTypeId} starting from {startingNumber}";

            var allCards = _cardService.GetAllBySet(setId, true).ToArray();
            var baseCards = allCards.Where(it => it.VariantTypeId == parentTypeId).ToDictionary(it => it.BaseId, it => it);
            var variantCards = allCards.Where(it => it.VariantTypeId == variantTypeId).OrderBy(it =>
            {
                if (!baseCards.TryGetValue(it.BaseId, out var baseCard))
                {
                    resultMessage += $"\r\nCould not find base card for {it.BaseId}";
                    return -1;
                }
                var baseCardId = baseCard.GetMultipleCardAttributeValue(attributeId)?.FirstOrDefault();
                if (baseCardId is null || !int.TryParse(baseCardId, out var parsedBaseCardId))
                {
                    resultMessage += $"\r\nCould not find attribute for card {it.BaseId}";
                    return -1;
                }
                return parsedBaseCardId;
            }).ToArray();
            foreach (var card in variantCards)
            {
                var proposedVariantId = startingNumber + variantCards.IndexOf(card);
                resultMessage += $"\r\nProposed id for variant {card.VariantId}: {proposedVariantId}";
            }
            return Ok(resultMessage);
        }

        [HttpPost]
        public IActionResult SetVariantIds(int setId, int siteId, int variantTypeId, int startingNumber, string attributeId, int? parentTypeId = null)
        {
            _siteAccessor.SetSiteId(siteId);

            var foundAttribute = _siteService.GetRoot().FirstChild<Data>()?.FirstChild<CardAttributeContainer>()?.FirstChild<CardAttribute>(it => it.Name == attributeId);
            if (foundAttribute is null)
            {
                throw new Exception("Could not find attribute container or attribute");
            }
            var allCards = _cardService.GetAllBySet(setId, true).ToArray();
            var baseCards = allCards.Where(it => it.VariantTypeId == parentTypeId).ToDictionary(it => it.BaseId, it => it);
            var variantCards = allCards.Where(it => it.VariantTypeId == variantTypeId).OrderBy(it =>
            {
                if (!baseCards.TryGetValue(it.BaseId, out var baseCard))
                {
                    return -1;
                }
                var baseCardId = baseCard.GetMultipleCardAttributeValue(attributeId)?.FirstOrDefault();
                if (baseCardId is null || !int.TryParse(baseCardId, out var parsedBaseCardId))
                {
                    return -1;
                }
                return parsedBaseCardId;
            }).ToArray();
            var updatedCards = 0;

            foreach (var card in variantCards)
            {
                var content = _contentService.GetById(card.VariantId)!;
                var newAttributes = new List<Dictionary<string, string>>();
                var attributesJson = content.GetValue("attributes");
                var attributes = !string.IsNullOrWhiteSpace(attributesJson?.ToString()) ? JsonConvert.DeserializeObject<Blocklist>(attributesJson.ToString()!) : new Blocklist();
                Dictionary<string, string>? existingIdAttribute = null;
                foreach (var attribute in attributes?.ContentData ?? Enumerable.Empty<Dictionary<string, string>>())
                {
                    newAttributes.Add(attribute);
                    if (attribute.TryGetValue("ability", out var abilityUdiString) &&
                        UdiParser.TryParse(abilityUdiString, out var abilityUdi) &&
                        (abilityUdi as GuidUdi)?.Guid == foundAttribute.Key)
                    {
                        existingIdAttribute = attribute;
                    }
                }
                if (existingIdAttribute is null)
                {
                    existingIdAttribute = new Dictionary<string, string>()
                        {
                            {"ability", Udi.Create(Constants.UdiEntityType.Document, foundAttribute.Key).ToString() },
                            {"contentTypeKey", new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26").ToString()}
                        };
                    newAttributes.Add(existingIdAttribute);
                }
                var proposedVariantId = startingNumber + variantCards.IndexOf(card);
                existingIdAttribute["value"] = proposedVariantId.ToString();

                content.SetValue("attributes", BlockListCreatorHelper.GetBlockListJsonFor(newAttributes!, new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26")));
                _contentService.SaveAndPublish(content);
                updatedCards++;
            }
            return Ok($"Updated {updatedCards} varians");
        }

        [HttpPost]
        public ActionResult MigrateDeltaPrices()
        {
            using var scope = _scopeProvider.CreateScope();

            var allRecords = scope.Database.Query<CardPriceRecordDBModel>(
                    scope.SqlContext.Sql()
                    .SelectAll()
                    .From<CardPriceRecordDBModel>())
                    .ToList();

            foreach (var cardGroup in allRecords.GroupBy(it => it.CardId))
            {
                foreach (var variantGroup in cardGroup.GroupBy(it => it.VariantId))
                {
                    var ordered = variantGroup.OrderBy(it => it.DateUtc).ToList();
                    for (var i = 0; i < ordered.Count; i++)
                    {
                        var record = ordered[i];
                        if (record.Delta > 0) continue;

                        record.Delta = i == 0 ? 0.0 : record.MainPrice - ordered[i - 1].MainPrice;
                        scope.Database.Update(record);
                    }
                }
            }
            scope.Complete();
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateOutdatedImages(int siteId)
        {
            _siteAccessor.SetSiteId(siteId);

            var needsNewImage = _siteService.GetRoot().FirstChild<Data>()?.FirstChild<CardContainer>()?.Children<Card>()?.Where(it => it.RequiresNewImage)?.ToArray() ?? [];

            var updatedCards = new List<string>();
            var skippedCards = new List<string>();

            var httpClient = new HttpClient();
            var accessToken = await new TcgPlayerAccessTokenGetter().Get(_cardGameSettingsConfig, httpClient);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            foreach (var card in needsNewImage)
            {
                var businessCard = _cardService.Get(card.Id);
                var tcgPlayerIdValues = businessCard?.GetMultipleCardAttributeValue("TcgPlayerId");
                var tcgPlayerId = tcgPlayerIdValues?.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(tcgPlayerId))
                {
                    var baseCard = _cardService.GetBaseVariants(businessCard.BaseId!)?.FirstOrDefault();
                    tcgPlayerId = baseCard?.GetMultipleCardAttributeValue("TcgPlayerId")?.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(tcgPlayerId))
                    {
                        skippedCards.Add($"{card.Name} (no TcgPlayerId)");
                        continue;
                    }
                }

                try
                {
                    var response = await httpClient.GetAsync($"https://api.tcgplayer.com/catalog/products/{tcgPlayerId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        skippedCards.Add($"{card.Name} (TCGPlayer API error: {response.StatusCode})");
                        continue;
                    }

                    var result = await response.Content.ReadFromJsonAsync<TcgPlayerResult<TcgPlayerProductModel>>();
                    var product = result?.Results.FirstOrDefault();
                    if (product is null || string.IsNullOrWhiteSpace(product.ImageUrl))
                    {
                        skippedCards.Add($"{card.Name} (no image found on TCGPlayer)");
                        continue;
                    }

                    var imageResponse = await httpClient.GetAsync(product.ImageUrl);
                    if (!imageResponse.IsSuccessStatusCode)
                    {
                        skippedCards.Add($"{card.Name} (failed to download image)");
                        continue;
                    }

                    IMedia media;
                    if (card.Image is not null)
                    {
                        media = _mediaService.GetById(card.Image.Id) ?? _mediaService.CreateMediaWithIdentity(card.Name, card.Image.Id, Image.ModelTypeAlias);
                    }
                    else
                    {
                        var cardImageParentId = _siteService.GetSettings().FirstChild<SiteSettings>().CardImageRoot.Id;
                        media = _mediaService.CreateMediaWithIdentity(card.Name, cardImageParentId, Image.ModelTypeAlias);
                    }

                    var extension = Path.GetExtension(product.ImageUrl);
                    if (string.IsNullOrWhiteSpace(extension)) extension = ".jpg";
                    var fileName = card.Name + extension;

                    using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                    media.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, "umbracoFile", fileName, imageStream);
                    _mediaService.Save(media);

                    if (card.Image is null)
                    {
                        var cardContent = _contentService.GetById(card.Id)!;
                        cardContent.SetValue("image", Udi.Create(Constants.UdiEntityType.Media, media.Key).ToString());
                        cardContent.SetValue("requiresNewImage", false);
                        _contentService.SaveAndPublish(cardContent);
                    }
                    else
                    {
                        var cardContent = _contentService.GetById(card.Id)!;
                        cardContent.SetValue("requiresNewImage", false);
                        _contentService.SaveAndPublish(cardContent);
                    }

                    updatedCards.Add(card.Name);
                    _logger.LogInformation($"Updated image for {card.Name} from TCGPlayer");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update image for {card.Name}");
                    skippedCards.Add($"{card.Name} (exception: {ex.Message})");
                }
            }

            var summary = $"Updated {updatedCards.Count} card(s): {string.Join(", ", updatedCards)}\n" +
                          $"Skipped {skippedCards.Count} card(s): {string.Join(", ", skippedCards)}";
            return Ok(summary);
        }
    }
}
