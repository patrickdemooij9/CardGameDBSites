using CardGameDBSites.API.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Enums;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/management")]
    [JwtAuthorization]
    public class ManagementApiController : Controller
    {
        private static readonly Guid CardsPresetElementTypeKey = new("c05466aa-127d-41d1-a42f-256fa02a6db8");
        private static readonly Guid CardsPresetItemElementTypeKey = new("18acdcbe-a990-430b-a76e-4f00301d2f46");

        private readonly DeckService _deckService;
        private readonly ISiteService _siteService;
        private readonly IContentService _contentService;

        public ManagementApiController(DeckService deckService, ISiteService siteService, IContentService contentService)
        {
            _deckService = deckService;
            _siteService = siteService;
            _contentService = contentService;
        }

        [HttpPost("decks/{deckId}/createPreset")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult CreatePresetFromDeck(int deckId)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true")
                return Forbid();

            var deck = _deckService.Get(deckId, DeckStatus.Published);
            if (deck is null) return NotFound();

            var collectionPage = _siteService.GetCollectionPage();
            if (collectionPage is null) return NotFound();

            var collectionPageContent = _contentService.GetById(collectionPage.Id);
            if (collectionPageContent is null) return NotFound();

            var groupedDeckCards = deck.Cards
                .Where(it => it.Amount > 0)
                .GroupBy(it => it.CardId)
                .Select(group => new { CardId = group.Key, Amount = group.Sum(it => it.Amount) })
                .ToArray();

            var cardContentById = _contentService.GetByIds(groupedDeckCards.Select(it => it.CardId))
                .ToDictionary(it => it.Id, it => it);

            var presetItemBlocks = groupedDeckCards
                .Select(group =>
                {
                    if (!cardContentById.TryGetValue(group.CardId, out var cardContent))
                    {
                        return null;
                    }

                    return new Dictionary<string, string>
                    {
                        ["card"] = Udi.Create(Constants.UdiEntityType.Document, cardContent.Key).ToString(),
                        ["amount"] = group.Amount.ToString()
                    };
                })
                .Where(it => it is not null)
                .Cast<Dictionary<string, string>>()
                .ToList();

            if (presetItemBlocks.Count == 0)
                return BadRequest("Could not create preset because no valid cards were found in this deck.");

            var itemBlockListJson = BlockListCreatorHelper.GetBlockListJsonFor(presetItemBlocks, CardsPresetItemElementTypeKey);

            var currentPresetJson = collectionPageContent.GetValue("presets")?.ToString();
            var currentPresetBlocks = new List<Dictionary<string, string>>();
            if (!string.IsNullOrWhiteSpace(currentPresetJson))
            {
                var deserializedBlockList = JsonConvert.DeserializeObject<BlockListCreatorHelper.Blocklist>(currentPresetJson!);
                if (deserializedBlockList?.ContentData is not null)
                {
                    currentPresetBlocks.AddRange(deserializedBlockList.ContentData);
                }
            }

            currentPresetBlocks.Add(new Dictionary<string, string>
            {
                ["displayName"] = deck.Name,
                ["items"] = itemBlockListJson
            });

            var updatedPresetJson = BlockListCreatorHelper.GetBlockListJsonFor(currentPresetBlocks, CardsPresetElementTypeKey);
            collectionPageContent.SetValue("presets", updatedPresetJson);
            _contentService.SaveAndPublish(collectionPageContent);

            return Ok();
        }
    }
}
