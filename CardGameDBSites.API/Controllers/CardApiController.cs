using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Cards;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using Umbraco.Cms.Core.Models;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [Route("/api/cards")]
    public class CardApiController : Controller
    {
        private readonly ICardSearchService _cardSearchService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;

        public CardApiController(ICardSearchService cardSearchService,
            ISiteAccessor siteAccessor,
            CardService cardService)
        {
            _cardSearchService = cardSearchService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
        }

        [HttpGet("all")]
        public IActionResult GetAll(int skip, int take)
        {
            var result = _cardSearchService.Search(new CardSearchQuery(take, _siteAccessor.GetSiteId())
            {
                Skip = skip
            }, out _);
            return Ok(result);
        }

        [HttpPost("byIds")]
        [ProducesResponseType(typeof(CardDetailApiModel[]), 200)]
        public IActionResult ByIds(int[] ids)
        {
            var cards = _cardService.Get(ids);
            return Ok(cards.Select(it => new CardDetailApiModel(it)));
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(PagedResult<CardDetailApiModel>), 200)]
        public IActionResult Query(CardsQueryPostApiModel model)
        {
            var result = _cardSearchService.Search(new CardSearchQuery(model.PageSize, _siteAccessor.GetSiteId())
            {
                Query = model.Query,
                Skip = model.PageSize * (model.PageNumber - 1),
                SetId = model.SetId,
                CustomFields = model.CustomFields
            }, out var totalItems).Select(it => new CardDetailApiModel(it));
            return Ok(new PagedResult<CardDetailApiModel>(totalItems, model.PageNumber, model.PageSize)
            {
                Items = result
            });
        }

        [HttpGet("getAllValues")]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetAllValues(string abilityName)
        {
            return Ok(_cardService.GetCardValues(abilityName));
        }
    }
}
