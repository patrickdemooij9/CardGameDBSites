using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services.Search;

namespace SkytearHorde.Business.Controllers.Api
{
    [ApiController]
    [Route("/api/cards")]
    public class CardApiController : Controller
    {
        private readonly ICardSearchService _cardSearchService;
        private readonly ISiteAccessor _siteAccessor;

        public CardApiController(ICardSearchService cardSearchService, ISiteAccessor siteAccessor)
        {
            _cardSearchService = cardSearchService;
            _siteAccessor = siteAccessor;
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
    }
}
