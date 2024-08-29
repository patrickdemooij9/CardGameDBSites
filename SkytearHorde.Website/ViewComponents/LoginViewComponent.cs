using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.ViewComponents
{
    public class LoginViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IPublishedContent currentPage)
        {
            var rootPage = currentPage.Root();
            var registerPage = rootPage.FirstChild<Register>();
            var forgotPasswordPage = rootPage.FirstChild<ForgotPassword>();

            return View("/Views/Partials/components/login.cshtml", new LoginViewModel(new LoginPostModel())
            {
                RegisterPage = registerPage,
                ForgotPasswordPage = forgotPasswordPage
            });
        }
    }
}
