using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System.Diagnostics;
using Umbraco.Cms.Core.Models;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/decks")]
    public class DecksApiController : Controller
    {
        private readonly DeckService _deckService;
        private readonly ISiteAccessor _siteAccessor;

        public DecksApiController(DeckService deckService, ISiteAccessor siteAccessor)
        {
            _deckService = deckService;
            _siteAccessor = siteAccessor;
        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(DeckApiModel), 200)]
        public IActionResult Get(int id)
        {
            var deck = _deckService.Get(id, DeckStatus.Published);
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
    }
}
