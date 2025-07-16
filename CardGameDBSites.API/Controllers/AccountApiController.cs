using CardGameDBSites.API.Models.Members;
using DeviceDetectorNET.Class.Device;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ResultModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/account")]
    public class AccountApiController : Controller
    {
        private readonly ISiteAccessor _siteAccessor;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly MemberInfoService _memberInfoService;
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly ILogger<AccountApiController> _logger;
        private readonly ISiteService _siteService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _globalConfig;
        private readonly CardGameSettingsConfig _config;

        public AccountApiController(ISiteAccessor siteAccessor, IMemberSignInManager memberSignInManager, MemberInfoService memberInfoService, IOptions<CardGameSettingsConfig> config, IMemberManager memberManager, IMemberService memberService, ILogger<AccountApiController> logger, ISiteService siteService, IEmailSender emailSender, IConfiguration globalConfig)
        {
            _siteAccessor = siteAccessor;
            _memberSignInManager = memberSignInManager;
            _memberInfoService = memberInfoService;
            _memberManager = memberManager;
            _memberService = memberService;
            _logger = logger;
            _siteService = siteService;
            _emailSender = emailSender;
            _globalConfig = globalConfig;
            _config = config.Value;
        }

        /*[HttpPost("Login")]
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
        }*/

        [HttpPost("Register")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Register(RegisterPostModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(_config.RecaptchaSecret))
            {
                var parameters = new Dictionary<string, string>
            {
                {"secret", _config.RecaptchaSecret },
                {"response", model.Recaptcha }
            };

                using var content = new FormUrlEncodedContent(parameters);
                var response = await new HttpClient().PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Recaptcha failed");
                else
                {
                    var responseContent = response.Content.ReadFromJsonAsync<RecaptchaVerifyResultModel>().Result;
                    if (!responseContent.Success)
                        return BadRequest("Recaptcha failed");
                }
            }

            var user = new MemberIdentityUser
            {
                UserName = $"{model.Email}_{_siteAccessor.GetSiteId()}",
                Email = model.Email,
                Name = model.UserName,
                IsApproved = true
            };
            var result = await _memberManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var newMember = _memberService.GetById(int.Parse(user.Id));
                newMember.SetValue("siteID", _siteAccessor.GetSiteId());
                _memberService.Save(newMember);

                var token = GetJwtToken(user, false);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }

            _logger.LogError("Could not create user due to: {0}", string.Join(",", result.Errors.Select(it => it.Description)));
            ModelState.AddModelError<RegisterPostModel>(it => it.Password, "Could not create the user. Do you perhaps already have an account with this email?");
            return BadRequest("Could not create the user. Do you perhaps already have an account with this email?");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordPostModel model)
        {
            var siteId = _siteAccessor.GetSiteId();
            var member = _memberService.GetAllMembers().FirstOrDefault(it => it.Email.Equals(model.Email) && it.GetValue<int>("siteID").Equals(siteId));

            if (member is null)
                return Ok();

            var resetCode = Guid.NewGuid();
            member.SetValue("resetCode", resetCode.ToString());
            member.SetValue("resetTimestamp", DateTime.UtcNow.AddHours(1));
            _memberService.Save(member);

            var siteSettings = _siteService.GetSettings().FirstChild<SiteSettings>();
            var rootUrl = _siteService.GetRoot().Url(mode: UrlMode.Absolute);

            await _emailSender.SendAsync(new EmailMessage(siteSettings.ForgotPasswordEmail, model.Email, "Password recovery", siteSettings.ForgotPasswordContent.ToString().Replace("[code]", resetCode.ToString()).Replace("/" +
                "[rootUrl]", rootUrl), true), "Contact");

            return Ok();
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(string), 200)]
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
                var token = GetJwtToken(member, model.RememberMe);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }

            ModelState.AddModelError("PostModel.Password", "Incorrect email/password");
            return Unauthorized("Incorrect email/password");
        }

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
                LikedDecks = member.LikedDecks
            });
        }

        private JwtSecurityToken GetJwtToken(MemberIdentityUser user, bool rememberMe)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_globalConfig["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Name!)
                };
            return new JwtSecurityToken(_globalConfig["Jwt:Issuer"],
                null,
                claims,
                expires: DateTime.Now.AddDays(rememberMe ? 30 : 1),
                signingCredentials: credentials);
        }
    }
}
