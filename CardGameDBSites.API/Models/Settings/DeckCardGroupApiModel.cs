using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings
{
    public class DeckCardGroupApiModel
    {
        public required string Header { get; set; }
        public bool HideAmount { get; set; }
        public RequirementApiModel[] Requirements { get; set; }
        public string[] Sorting { get; set; }

        public DeckCardGroupApiModel()
        {
            Requirements = [];
            Sorting = [];
        }
    }
}
