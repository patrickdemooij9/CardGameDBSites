using CardGameDBSites.API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/member")]
    public class MemberController : Controller
    {
        private readonly MemberInfoService _memberInfoService;

        public MemberController(MemberInfoService memberInfoService)
        {
            _memberInfoService = memberInfoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(MemberApiModel), 200)]
        public IActionResult Get(int memberId)
        {
            var memberName = _memberInfoService.GetName(memberId);
            if (string.IsNullOrWhiteSpace(memberName)) return NotFound();

            return Ok(new MemberApiModel { DisplayName = memberName });
        }
    }
}
