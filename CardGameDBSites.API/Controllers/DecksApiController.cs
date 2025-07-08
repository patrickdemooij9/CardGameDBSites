using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
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
    public class DecksApiController : Controller
    {
        private readonly DeckService _deckService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IMemberManager _memberManager;

        public DecksApiController(DeckService deckService, ISiteAccessor siteAccessor, IMemberManager memberManager)
        {
            _deckService = deckService;
            _siteAccessor = siteAccessor;
            _memberManager = memberManager;
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
            return Ok(new DeckApiModel(deck));
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(PagedResult<DeckApiModel>), 200)]
        public IActionResult Query(DeckQueryPostModel query)
        {
            var decks = _deckService.GetAll(new DeckPagedRequest(query.TypeId)
            {
                SiteId = _siteAccessor.GetSiteId(),
                Status = DeckStatus.Published,
                Cards = query.Cards,
                Take = query.Take,
                Page = query.Page,
                UserId = query.UserId,
                OrderBy = query.OrderBy
            });
            return Ok(decks);
        }

        [HttpPost("likeDeck")]
        [EnableCors("api-login")] //TODO: Rework the authorization correctly
        public async Task<IActionResult> LikeDeck(int deckId)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var deck = _deckService.Get(deckId);
            if (deck is null) return Unauthorized();

            _deckService.ToggleLikeDeck(deck, int.Parse(currentUser.Id));
            return Ok();
        }
    }
}
