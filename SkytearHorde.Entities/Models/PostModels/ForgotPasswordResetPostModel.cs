using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class ForgotPasswordResetPostModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        [DisplayName("New password")]
        public string NewPassword { get; set; }
    }
}
