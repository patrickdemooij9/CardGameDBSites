using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public string Code { get; set; }
        public bool SuccessPostModel { get; set; }
        public bool SuccessResetModel { get; set; }

        public ForgotPasswordPostModel PostModel { get; set; }
        public ForgotPasswordResetPostModel ResetPostModel { get; set; }
    }
}
