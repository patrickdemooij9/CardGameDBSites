using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderDeckCardGroupApiModel
    {
        public required string DisplayName { get; set; }
        public string? SortBy { get; set; }
        public RequirementApiModel[] Requirements { get; set; }

        public DeckBuilderDeckCardGroupApiModel()
        {
            Requirements = [];
        }
    }
}
