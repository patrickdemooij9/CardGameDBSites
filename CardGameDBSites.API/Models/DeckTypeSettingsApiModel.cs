using CardGameDBSites.API.Models.Requirements;
using CardGameDBSites.API.Models.Settings;

namespace CardGameDBSites.API.Models
{
    public class DeckTypeSettingsApiModel
    {
        public required string OverviewUrl { get; set; }
        public required string DisplayName { get; set; }
        public int AmountOfSquadCards { get; set; }

        public DeckCardGroupApiModel[] Groupings { get; set; } = [];
        public DeckActionApiModel[] Actions { get; set; } = [];
        public DeckCardImageRuleApiModel[] ImageRules { get; set; } = [];
        public RequirementApiModel[] MainCardRequirements { get; set; } = [];
    }
}
