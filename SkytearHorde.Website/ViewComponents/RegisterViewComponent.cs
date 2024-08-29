using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.ViewComponents
{
    public class RegisterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("/Views/Partials/components/register.cshtml", new RegisterPostModel());
        }
    }
}
