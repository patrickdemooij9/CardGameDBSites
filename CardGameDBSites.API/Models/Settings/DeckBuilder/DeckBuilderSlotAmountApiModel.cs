namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderSlotAmountApiModel
    {
        public string Type { get; set; }
        public Dictionary<string, object> Config { get; set; }

        public DeckBuilderSlotAmountApiModel(string type)
        {
            Type = type;
            Config = [];
        }
    }
}
