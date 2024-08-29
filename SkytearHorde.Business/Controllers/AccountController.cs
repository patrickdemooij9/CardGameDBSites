using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ResultModels;
using System.Net.Http.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Controllers
{
    public class AccountController : SurfaceController
    {
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IEmailSender _emailSender;
        private readonly ISiteService _siteService;
        private readonly ILogger _logger;

        public AccountController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IMemberSignInManager memberSignInManager, IMemberManager memberManager, IEmailSender emailSender, IMemberService memberService, ISiteAccessor siteAccessor, ISiteService siteService, ILogger<AccountController> logger) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberSignInManager = memberSignInManager;
            _memberManager = memberManager;
            _emailSender = emailSender;
            _memberService = memberService;
            _siteAccessor = siteAccessor;
            _siteService = siteService;
            _logger = logger;
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([Bind(Prefix = "PostModel")] LoginPostModel login)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var userName = $"{login.Email}_{_siteAccessor.GetSiteId()}";
            var loginResult = await _memberSignInManager.PasswordSignInAsync(userName, login.Password, login.RememberMe, false);
            if (loginResult.Succeeded)
            {
                return Redirect(CurrentPage.Root().Url());
            }

            ModelState.AddModelError("PostModel.Password", "Incorrect email/password");
            return CurrentUmbracoPage();
        }

        public async Task<IActionResult> Register(RegisterPostModel register)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var parameters = new Dictionary<string, string>
            {
                {"secret", "" },
                {"response", register.Recaptcha }
            };

            using var content = new FormUrlEncodedContent(parameters);
            var response = await new HttpClient().PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (!response.IsSuccessStatusCode)
                return CurrentUmbracoPage();
            else
            {
                var responseContent = response.Content.ReadFromJsonAsync<RecaptchaVerifyResultModel>().Result;
                if (!responseContent.Success)
                    return CurrentUmbracoPage();
            }

            var user = new MemberIdentityUser
            {
                UserName = $"{register.Email}_{_siteAccessor.GetSiteId()}",
                Email = register.Email,
                Name = register.UserName,
                IsApproved = true
            };
            var result = await _memberManager.CreateAsync(user, register.Password);
            if (result.Succeeded)
            {
                var newMember = _memberService.GetById(int.Parse(user.Id));
                newMember.SetValue("siteID", _siteAccessor.GetSiteId());
                _memberService.Save(newMember);

                await _memberSignInManager.SignInAsync(user, false);

                return Redirect(CurrentPage.Root().Url());
            }

            _logger.LogError("Could not create user due to: {0}", string.Join(",", result.Errors.Select(it => it.Description)));
            ModelState.AddModelError<RegisterPostModel>(it => it.Password, "Could not create the user. Do you perhaps already have an account with this email?");
            return CurrentUmbracoPage();
        }

        public async Task<IActionResult> ForgotPassword([Bind(Prefix = "PostModel")]ForgotPasswordPostModel model)
        {
            var siteId = _siteAccessor.GetSiteId();
            var member = _memberService.GetAllMembers().FirstOrDefault(it => it.Email.Equals(model.Email) && it.GetValue<int>("siteID").Equals(siteId));
            TempData["Success"] = true; //Always set true to make sure people can't find out if there are accounts.

            if (member is null)
                return CurrentUmbracoPage();

            var resetCode = Guid.NewGuid();
            member.SetValue("resetCode", resetCode.ToString());
            member.SetValue("resetTimestamp", DateTime.UtcNow.AddHours(1));
            _memberService.Save(member);

            var siteSettings = _siteService.GetSettings().FirstChild<SiteSettings>();
            var rootUrl = UmbracoContext.PublishedRequest.PublishedContent!.Root().Url(mode: UrlMode.Absolute);

            await _emailSender.SendAsync(new EmailMessage(siteSettings.ForgotPasswordEmail, model.Email, "Password recovery", siteSettings.ForgotPasswordContent.ToString().Replace("[code]", resetCode.ToString()).Replace("/" +
                "[rootUrl]", rootUrl), true), "Contact");

            return CurrentUmbracoPage();
        }

        public async Task<IActionResult> ResetPassword([Bind(Prefix = "ResetPostModel")] ForgotPasswordResetPostModel model)
        {
            var memberByCode = _memberService.GetAllMembers().FirstOrDefault(it => it.GetValue<string>("resetCode")?.Equals(model.Code) is true);
            if (memberByCode is null || memberByCode.GetValue<DateTime>("resetTimestamp") < DateTime.UtcNow)
                return CurrentUmbracoPage();

            var user = await _memberManager.FindByIdAsync(memberByCode.Id.ToString());
            var resetCode = await _memberManager.GeneratePasswordResetTokenAsync(user);
            await _memberManager.ResetPasswordAsync(user, resetCode, model.NewPassword);

            TempData["Success"] = true;
            return CurrentUmbracoPage();
        }
    }
}
