using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class VerifyPackViewModel
    {
        public required int SetId { get; set; }
        public required Card[] Cards { get; set; }
        public required VariantType[] VariantTypes { get; set; }
    }
}
