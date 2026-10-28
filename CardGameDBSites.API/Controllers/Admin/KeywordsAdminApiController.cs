using CardGameDBSites.API.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Keywords;

namespace CardGameDBSites.API.Controllers.Admin
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/admin/keywords")]
    [JwtAuthorization]
    [ApiExplorerSettings(GroupName = "Admin")]
    public class KeywordsAdminApiController : Controller
    {
        private readonly CardKeywordService _cardKeywordService;

        public KeywordsAdminApiController(CardKeywordService cardKeywordService)
        {
            _cardKeywordService = cardKeywordService;
        }

        [HttpPost("backfill")]
        [ProducesResponseType(typeof(KeywordBackfillResult), 200)]
        public IActionResult Backfill(bool dryRun = true)
        {
            if (HttpContext.User.FindFirst("isAdmin")?.Value != "true") return Forbid();

            return Ok(_cardKeywordService.Backfill(dryRun));
        }
    }
}
