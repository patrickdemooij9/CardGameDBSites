using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DeckCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public int StartingAmount { get; set; } = 1;
        public int MaxAmount { get; set; }
        public string Ability { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Rarity { get; set; }
        public string OverviewImage { get; set; }
        public AbilityGroupViewModel[] AbilityGroups { get; set; }

        public DeckCardViewModel()
        {
            AbilityGroups = Array.Empty<AbilityGroupViewModel>();
        }
    }
}
