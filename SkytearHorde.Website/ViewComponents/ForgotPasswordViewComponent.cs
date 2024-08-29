using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.ViewComponents
{
    public class ForgotPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var code = Request.Query["code"];
            var isSuccess = TempData["success"] is true;

            var model = new ForgotPasswordViewModel()
            {
                Code = code,
                SuccessPostModel = string.IsNullOrWhiteSpace(code) && isSuccess,
                SuccessResetModel = !string.IsNullOrWhiteSpace(code) && isSuccess,
                PostModel = new ForgotPasswordPostModel(),
                ResetPostModel = new ForgotPasswordResetPostModel
                {
                    Code = code
                }
            };

            return View("/Views/Partials/components/forgotPassword.cshtml", model);
        }
    }
}
