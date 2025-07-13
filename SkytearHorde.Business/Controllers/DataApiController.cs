using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using Umbraco.Cms.Web.Common.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class DataApiController : UmbracoApiController
    {
        private readonly CardService _cardService;
        private readonly ISiteAccessor _siteAccessor;

        public DataApiController(CardService cardService, ISiteAccessor siteAccessor)
        {
            _cardService = cardService;
            _siteAccessor = siteAccessor;
        }

        public IActionResult SearchCards(string term)
        {
            var cards = _cardService.Search(new CardSearchQuery(10, _siteAccessor.GetSiteId())
            {
                Query = term,
                IncludeHideFromDeck = false,
                VariantTypeId = 0
            }, out _);
            return Ok(cards.Select(it => new
            {
                value = it.BaseId,
                label = it.DisplayName
            }));
        }
    }
}
