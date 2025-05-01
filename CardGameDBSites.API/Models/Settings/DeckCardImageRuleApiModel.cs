using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings
{
    public class DeckCardImageRuleApiModel
    {
        public required string ImageUrl { get; set; }
        public RequirementApiModel[] Requirements { get; set; }

        public DeckCardImageRuleApiModel()
        {
            Requirements = [];
        }
    }
}
