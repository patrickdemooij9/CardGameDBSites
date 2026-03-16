using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Database;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
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

        public ManagementController(CardService cardService, IContentService contentService, ISiteAccessor siteAccessor, ISiteService siteService)
        {
            _cardService = cardService;
            _contentService = contentService;
            _siteAccessor = siteAccessor;
            _siteService = siteService;
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
    }
}
