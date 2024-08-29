using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class ForgotPasswordPostModel
    {
        [Required]
        [DisplayName("Email")]
        public string Email { get; set; }
    }
}
