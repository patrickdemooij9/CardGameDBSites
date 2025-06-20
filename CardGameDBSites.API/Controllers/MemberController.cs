using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Members;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api-login")]
    [Route("/api/member")]
    public class MemberController : Controller
    {
        private readonly MemberInfoService _memberInfoService;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IMemberManager _memberManager;
        private readonly IConfiguration _config;

        public MemberController(MemberInfoService memberInfoService,
            IMemberSignInManager memberSignInManager,
            ISiteAccessor siteAccessor,
            IMemberManager memberManager,
            IConfiguration config)
        {
            _memberInfoService = memberInfoService;
            _memberSignInManager = memberSignInManager;
            _siteAccessor = siteAccessor;
            _memberManager = memberManager;
            _config = config;
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(CurrentMemberApiModel), 200)]
        public async Task<IActionResult> Login(LoginPostModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = $"{model.Email}_{_siteAccessor.GetSiteId()}";
            var loginResult = await _memberSignInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, false);
            if (loginResult.Succeeded)
            {
                return GetCurrentMember();
            }

            ModelState.AddModelError("PostModel.Password", "Incorrect email/password");
            return Unauthorized("Incorrect email/password");
        }

        /*[HttpPost("Login")]
        public async Task<IActionResult> Login(LoginPostModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = $"{model.Email}_{_siteAccessor.GetSiteId()}";
            var loginResult = await _memberManager.ValidateCredentialsAsync(userName, model.Password);
            if (loginResult)
            {
                var member = await _memberManager.FindByNameAsync(userName);
                if (member is null)
                {
                    return Unauthorized("Incorrect email/password");
                }
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, member.Id),
                    new Claim(ClaimTypes.Name, member.Name!)
                };
                var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                    null,
                    claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credentials);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }

            ModelState.AddModelError("PostModel.Password", "Incorrect email/password");
            return Unauthorized("Incorrect email/password");
        }*/

        [HttpGet("IsLoggedIn")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult IsLoggedIn()
        {
            var test = _memberManager.GetCurrentMemberAsync().Result;
            return Ok("You need to be authenticated to see this message.");
        }

        [HttpGet("GetCurrentMember")]
        [ProducesResponseType(typeof(CurrentMemberApiModel), 200)]
        public IActionResult GetCurrentMember()
        {
            var member = _memberInfoService.GetMemberInfo();
            if (member is null) return Unauthorized("You need to be logged in to see this information.");

            return Ok(new CurrentMemberApiModel
            {
                Id = member.Id,
                DisplayName = member.DisplayName,
            });
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
