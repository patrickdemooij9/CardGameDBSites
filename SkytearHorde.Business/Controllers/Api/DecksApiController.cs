using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Business.Controllers.Api
{
    [ApiController]
    [EnableCors("api")]
    [Route("api/decks")]
    public class DecksApiController : Controller
    {
        private readonly DeckService _deckService;

        public DecksApiController(DeckService deckService)
        {
            _deckService = deckService;
        }

        [HttpGet("get")]
        public IActionResult Get(int id)
        {
            var deck = _deckService.Get(id, DeckStatus.Published);
            if (deck is null)
            {
                return NotFound();
            }
            return Ok(deck);
        }
    }
}
