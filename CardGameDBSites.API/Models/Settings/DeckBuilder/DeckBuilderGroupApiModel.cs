using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderGroupApiModel
    {
        public required int Id { get; set; }
        public string? Name { get; set; }
        public RequirementApiModel[] Requirements { get; set; }
        public DeckBuilderSlotApiModel[] Slots { get; set; } = [];

        public DeckBuilderGroupApiModel()
        {
            Requirements = [];
        }
    }
}
