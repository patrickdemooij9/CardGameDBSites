using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class LoginViewModel
    {
        public LoginPostModel PostModel { get; set; }
        
        public Register? RegisterPage { get; set; }
        public ForgotPassword? ForgotPasswordPage { get; set; }

        public LoginViewModel(LoginPostModel postModel)
        {
            PostModel = postModel;
        }
    }
}
