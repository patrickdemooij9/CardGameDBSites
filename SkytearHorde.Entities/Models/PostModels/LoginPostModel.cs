using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class LoginPostModel
    {
        [DisplayName("Email")]
        [Required]
        public string Email { get; set; }

        [DisplayName("Password")]
        [Required]
        public string Password { get; set; }

        [DisplayName("Remember me?")]
        public bool RememberMe { get; set; }
    }
}
