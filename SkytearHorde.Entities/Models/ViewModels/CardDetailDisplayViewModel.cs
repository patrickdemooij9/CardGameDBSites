using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardDetailDisplayViewModel
    {
        public IAbilityValue AbilityValue { get; set; }
        public bool ShowAsTags { get; set; }
        public string? OverviewPageUrl { get; set; }
    }
}
