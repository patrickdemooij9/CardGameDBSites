namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderApiModel
    {
        public DeckBuilderGroupApiModel[] Groups { get; set; }

        public DeckBuilderApiModel()
        {
            Groups = [];
        }
    }
}
