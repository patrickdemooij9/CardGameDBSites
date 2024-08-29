

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class RegisterPostModel
    {
        [DisplayName("Username")]
        [Required]
        public string UserName { get; set; }

        [DisplayName("Email")]
        [Required]
        public string Email { get; set; }

        [DisplayName("Password")]
        [Required]
        [MinLength(10, ErrorMessage = "Your password should be 10 characters long")]
        public string Password { get; set; }

        [DisplayName("TermsAndConditions")]
        [Required]
        public bool TermAndConditions { get; set; }

        [Required]
        public string Recaptcha { get; set; }
    }
}
