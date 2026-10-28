namespace CardGameDBSites.API.Models.Cards
{
    public class KeywordValuesApiModel
    {
        public required string[] TriggeredAbilities { get; set; }
        public required string[] Keywords { get; set; }
    }
}
