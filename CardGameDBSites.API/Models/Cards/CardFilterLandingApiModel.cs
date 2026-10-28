namespace CardGameDBSites.API.Models.Cards
{
    public class CardFilterLandingApiModel
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string Value { get; set; }
        public int CardCount { get; set; }
    }
}
