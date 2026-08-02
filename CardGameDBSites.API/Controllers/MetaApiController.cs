using CardGameDBSites.API.Helpers;
using CardGameDBSites.API.Models.Decks;
using CardGameDBSites.API.Models.Meta;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Web;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/meta")]
    [ApiExplorerSettings(GroupName = "Meta")]
    public class MetaApiController : Controller
    {
        private readonly PeriodRepository _periodRepository;
        private readonly MetaSnapshotService _metaSnapshotService;
        private readonly MetaCardPageService _metaCardPageService;
        private readonly CardPageService _cardPageService;
        private readonly CardService _cardService;
        private readonly TournamentService _tournamentService;
        private readonly DeckService _deckService;
        private readonly CardPriceService _cardPriceService;
        private readonly SettingsService _settingsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public MetaApiController(
            PeriodRepository periodRepository,
            MetaSnapshotService metaSnapshotService,
            MetaCardPageService metaCardPageService,
            CardPageService cardPageService,
            CardService cardService,
            TournamentService tournamentService,
            DeckService deckService,
            CardPriceService cardPriceService,
            SettingsService settingsService,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _periodRepository = periodRepository;
            _metaSnapshotService = metaSnapshotService;
            _metaCardPageService = metaCardPageService;
            _cardPageService = cardPageService;
            _cardService = cardService;
            _tournamentService = tournamentService;
            _deckService = deckService;
            _cardPriceService = cardPriceService;
            _settingsService = settingsService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        /// <summary>
        /// Resolves the card a MetaCardDetail page is about from the requested URL path (the same
        /// derivation the content path resolver uses to route the page).
        /// </summary>
        [HttpGet("resolve-card")]
        [ProducesResponseType(typeof(MetaCardApiModel), 200)]
        [ProducesResponseType(404)]
        public IActionResult ResolveCard([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return NotFound();

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var card = _metaCardPageService.ResolveCard(path);
            if (card is null) return NotFound();

            return Ok(new MetaCardApiModel
            {
                BaseId = card.BaseId,
                DisplayName = card.DisplayName,
                UrlSegment = _cardPageService.GetUrl(card),
                ImageUrl = card.Image is null ? null : ImageCropHelper.ToApiModels(card.Image)
            });
        }

        /// <summary>
        /// The meta detail page URL for a card, when the card is a leader (has a meta page). Returns a null
        /// Url otherwise. Used to link from the card page to its meta page.
        /// </summary>
        [HttpGet("card-page-url")]
        [ProducesResponseType(typeof(MetaCardLinkApiModel), 200)]
        public IActionResult GetCardPageUrl([FromQuery] int cardId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var card = _cardService.Get(cardId);
            var url = card is null ? null : _metaCardPageService.GetMetaUrlForCard(card);
            return Ok(new MetaCardLinkApiModel { Url = url });
        }

        /// <summary>
        /// Returns persisted usage/winrate for the given cards in a period, read from the snapshot. Every
        /// requested id gets an entry; ids without snapshot data report zeros. Percentages are unrounded.
        /// </summary>
        [HttpGet("card-stats")]
        [ProducesResponseType(typeof(MetaCardStatApiModel[]), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetCardStats([FromQuery] int periodId, [FromQuery] int[] cardIds)
        {
            var period = _periodRepository.GetById(periodId);
            if (period is null) return NotFound();

            var stats = _metaSnapshotService.GetCardStats(period.SiteId, period.FormatId, period.Id, cardIds ?? []);

            var result = stats.Select(s => new MetaCardStatApiModel
            {
                CardId = s.CardId,
                DeckCount = s.DeckCount,
                UsagePercentage = s.UsagePercentage,
                WinratePercentage = s.WinratePercentage
            }).ToArray();

            return Ok(result);
        }

        /// <summary>
        /// The "Top meta decks" for a leader: tournament decks piloting it in a period, best finish first.
        /// </summary>
        [HttpGet("top-decks")]
        [ProducesResponseType(typeof(DeckApiModel[]), 200)]
        public IActionResult GetTopDecks([FromQuery] int periodId, [FromQuery] int cardId, [FromQuery] int count = 6, [FromQuery] int leaderGroupId = 1, [FromQuery] int leaderSlotId = 0)
        {
            var deckIds = _tournamentService.GetTopDeckIdsForLeader(periodId, cardId, count, leaderGroupId, leaderSlotId).ToArray();
            if (deckIds.Length == 0) return Ok(Array.Empty<DeckApiModel>());

            var decksById = _deckService.Get(deckIds, DeckStatus.Published).ToDictionary(d => d.Id);
            var allowPricing = _settingsService.GetSiteSettings().AllowPricing;

            // Preserve the placement ordering from the query.
            var result = deckIds
                .Where(decksById.ContainsKey)
                .Select(id =>
                {
                    var deck = decksById[id];
                    var price = allowPricing ? new DeckPriceApiModel { MarketPrice = _cardPriceService.GetPriceByDeck(deck) } : null;
                    return new DeckApiModel(deck, price);
                })
                .ToArray();

            return Ok(result);
        }
    }
}
