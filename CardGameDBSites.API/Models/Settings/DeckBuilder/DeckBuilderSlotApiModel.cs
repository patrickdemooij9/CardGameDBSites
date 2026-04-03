using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderSlotApiModel
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public DeckBuilderDeckCardGroupApiModel[] CardGroups { get; set; }

        public int MinCards { get; set; }
        public required DeckBuilderSlotAmountApiModel MaxCardAmount {get; set;}
        public string DisplaySize { get; set; } = "Small";
        public bool DisableRemoval { get; set; }
        public bool NumberMode { get; set; }
        public int? ShowIfTargetSlotIsFilled { get; set; }
        public RequirementApiModel[] Requirements { get; set; }

        public DeckBuilderSlotApiModel()
        {
            CardGroups = [];
            Requirements = [];
        }
    }
}
