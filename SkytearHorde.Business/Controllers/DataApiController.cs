using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services.Search;
using Umbraco.Cms.Web.Common.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class DataApiController : UmbracoApiController
    {
        private readonly ICardSearchService _cardSearchService;
        private readonly ISiteAccessor _siteAccessor;

        public DataApiController(ICardSearchService cardSearchService, ISiteAccessor siteAccessor)
        {
            _cardSearchService = cardSearchService;
            _siteAccessor = siteAccessor;
        }

        public IActionResult SearchCards(string term)
        {
            var cards = _cardSearchService.Search(new CardSearchQuery(10, _siteAccessor.GetSiteId())
            {
                Query = term,
                IncludeHideFromDeck = false,
                VariantTypeId = 0
            });
            return Ok(cards.Select(it => new
            {
                value = it.BaseId,
                label = it.DisplayName
            }));
        }
    }
}
