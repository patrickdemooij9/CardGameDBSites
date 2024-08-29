using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Utils;
using Umbraco.Cms.Web.Common.Controllers;

namespace SkytearHorde.Business.Controllers.Api
{
    public class DeckApiController : UmbracoApiController
    {
        private readonly DeckService _deckService;
        private readonly CardService _cardService;

        public DeckApiController(DeckService deckService, CardService cardService)
        {
            _deckService = deckService;
            _cardService = cardService;
        }

        public IActionResult Get(int id)
        {
            var deck = _deckService.Get(id);
            if (deck is null) return NotFound();

            var jsonModel = DeckJsonFileHelper.ToJsonModel(deck, _cardService);
            return Ok(jsonModel);
        }
    }
}
