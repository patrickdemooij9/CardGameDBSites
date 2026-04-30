using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/meta")]
    public class MetaApiController : Controller
    {
        private readonly MetaService _metaService;
        private readonly ArchetypeService _archetypeService;

        public MetaApiController(MetaService metaService, ArchetypeService archetypeService)
        {
            _metaService = metaService;
            _archetypeService = archetypeService;
        }

        [HttpGet("{siteId:int}/top-decks")]
        public IActionResult GetTopDecks(int siteId, int? formatId = null)
        {
            var results = _metaService.GetTopDecks(siteId, formatId);
            return Ok(results);
        }

        [HttpGet("{siteId:int}/trending")]
        public IActionResult GetTrending(
            int siteId,
            int? formatId = null,
            DateTime? currentStart = null,
            DateTime? currentEnd = null,
            DateTime? previousStart = null,
            DateTime? previousEnd = null)
        {
            var now = DateTime.UtcNow;
            var cStart = currentStart ?? now.AddDays(-14);
            var cEnd = currentEnd ?? now;
            var pStart = previousStart ?? now.AddDays(-28);
            var pEnd = previousEnd ?? now.AddDays(-14);

            var results = _metaService.GetTrendingDecks(siteId, formatId, cStart, cEnd, pStart, pEnd);
            return Ok(results);
        }

        [HttpGet("{siteId:int}/archetypes")]
        public IActionResult GetArchetypes(int siteId, int? formatId = null)
        {
            var archetypes = _archetypeService.GetArchetypesForSite(siteId, formatId);
            return Ok(archetypes);
        }
    }
}
