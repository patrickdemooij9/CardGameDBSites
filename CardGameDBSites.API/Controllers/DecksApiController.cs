using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.Decks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Processors;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System.Diagnostics;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/decks")]
    [OptionalJwtAuthorization]
    [ApiExplorerSettings(GroupName = "Decks")]
    public class DecksApiController : Controller
    {
        private readonly DeckService _deckService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IMemberManager _memberManager;
        private readonly CardPriceService _cardPriceService;
        private readonly DeckTrackingProcessor _deckTrackingProcessor;
        private readonly SettingsService _settingsService;

        public DecksApiController(
            DeckService deckService,
            ISiteAccessor siteAccessor,
            IMemberManager memberManager,
            CardPriceService cardPriceService,
            DeckTrackingProcessor deckTrackingProcessor,
            SettingsService settingsService)
        {
            _deckService = deckService;
            _siteAccessor = siteAccessor;
            _memberManager = memberManager;
            _cardPriceService = cardPriceService;
            _deckTrackingProcessor = deckTrackingProcessor;
            _settingsService = settingsService;
        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(DeckApiModel), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var deck = _deckService.Get(id, DeckStatus.Published);
            if (_memberManager.IsLoggedIn())
            {
                var userId = int.Parse((await _memberManager.GetCurrentMemberAsync())!.Id);
                var savedDeck = _deckService.Get(id, DeckStatus.Saved);
                if (savedDeck != null && savedDeck.CreatedBy == userId && savedDeck.CreatedDate > (deck?.CreatedDate ?? DateTime.MinValue))
                {
                    deck = savedDeck;
                }
            }
            if (deck is null)
            {
                return NotFound();
            }

            DeckPriceApiModel? priceModel = null;
            if (_settingsService.GetSiteSettings().AllowPricing)
            {
                priceModel = new DeckPriceApiModel { MarketPrice = _cardPriceService.GetPriceByDeck(deck) };
            }
            return Ok(new DeckApiModel(deck, priceModel));
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(PagedResult<DeckApiModel>), 200)]
        public async Task<IActionResult> Query(DeckQueryPostModel query)
        {
            var status = DeckStatus.Published;
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            int? useUserCollection = null;
            if (_memberManager.IsLoggedIn() && currentUser != null)
            {
                if (query.UserId.HasValue && currentUser.Id == query.UserId.ToString())
                {
                    status = query.Status;
                }
                
                if (query.OrderBy == "collection")
                {
                    useUserCollection = int.Parse(currentUser.Id);
                }
            }

            var decks = _deckService.GetAll(new DeckPagedRequest(query.TypeId)
            {
                SiteId = _siteAccessor.GetSiteId(),
                Status = status,
                Cards = query.Cards,
                Take = query.Take,
                Page = query.Page,
                UserId = query.UserId,
                OrderBy = query.OrderBy,
                DateFrom = query.DateFrom,
                DateTo = query.DateTo,
                UseUserCollectionId = useUserCollection
            });

            var allowPricing = _settingsService.GetSiteSettings().AllowPricing;
                return Ok(new PagedResult<DeckApiModel>(decks.TotalItems, query.Page, query.Take)
            {
                Items = decks.Items?.Select(deck =>
                {
                    return new DeckApiModel(deck, !allowPricing ? null : new DeckPriceApiModel { MarketPrice = _cardPriceService.GetPriceByDeck(deck) });
                 }).ToArray() ?? []
            });
        }

        [HttpPost("likeDeck")]
        public async Task<IActionResult> LikeDeck([FromBody] int deckId)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var deck = _deckService.Get(deckId);
            if (deck is null) return Unauthorized();

            _deckService.ToggleLikeDeck(deck, int.Parse(currentUser.Id));
            return Ok();
        }

        [HttpPost("viewDeck")]
        public async Task<IActionResult> ViewDeck([FromBody] int deckId)
        {
            _deckTrackingProcessor.AddDeckView(deckId);
            return Ok();
        }

        [HttpDelete("deleteDeck")]
        [JwtAuthorization]
        public async Task<IActionResult> DeleteDeck(int deckId)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var userId = int.Parse(currentUser.Id);
            var deck = _deckService.Get(deckId);
            if (deck is null) return NotFound();

            if (deck.CreatedBy != userId) return Unauthorized();

            _deckService.DeleteDeck(deck);
            return Ok();
        }
    }
}
