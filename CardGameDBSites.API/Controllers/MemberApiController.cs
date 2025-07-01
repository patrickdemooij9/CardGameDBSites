using CardGameDBSites.API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/member")]
    public class MemberApiController : Controller
    {
        private readonly MemberInfoService _memberInfoService;

        public MemberApiController(MemberInfoService memberInfoService)
        {
            _memberInfoService = memberInfoService;
        }

        [HttpGet("byId")]
        [ProducesResponseType(typeof(MemberApiModel), 200)]
        public IActionResult GetById(int memberId)
        {
            var member = _memberInfoService.Get(memberId);
            if (member is null) return NotFound();

            return Ok(new MemberApiModel { Id = member.Id, DisplayName = member.DisplayName });
        }

        [HttpPost("byIds")]
        [ProducesResponseType(typeof(MemberApiModel[]), 200)]
        public IActionResult GetByIds(int[] memberIds)
        {
            return Ok(memberIds.Select(it => _memberInfoService.Get(it)).WhereNotNull().Select(it => new MemberApiModel { Id = it.Id, DisplayName = it.DisplayName }));
        }
    }
}
