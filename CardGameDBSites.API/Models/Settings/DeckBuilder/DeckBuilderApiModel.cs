namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderApiModel
    {
        public required int Id { get; set; }
        public DeckBuilderGroupApiModel[] Groups { get; set; }
        public int MaxDynamicSlots { get; set; }
        public bool PreselectFirstSlot { get; set; }

        public DeckBuilderApiModel()
        {
            Groups = [];
        }
    }
}
