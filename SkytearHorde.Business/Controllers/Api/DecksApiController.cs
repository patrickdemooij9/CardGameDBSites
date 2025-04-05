using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace SkytearHorde.Business.Controllers.Api
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
        [ProducesResponseType(typeof(DeckViewModel), 200)]
        public IActionResult Get(int id)
        {
            var deck = _deckService.Get(id, DeckStatus.Published);
            if (deck is null)
            {
                return NotFound();
            }
            return Ok(new DeckViewModel(deck));
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(PagedResult<DeckViewModel>), 200)]
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
