using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderApiModel
    {
        public required int Id { get; set; }
        public string[] DefaultNames { get; set; }
        public int? OverwriteAmount { get; set; }
        public RequirementApiModel[] Requirements { get; set; }
        public DeckBuilderGroupApiModel[] Groups { get; set; }
        public DeckBuilderGroupApiModel[] SideboardGroups { get; set; }

        public DeckBuilderApiModel()
        {
            DefaultNames = [];
            Requirements = [];
            Groups = [];
            SideboardGroups = [];
        }
    }
}
