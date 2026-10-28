namespace CardGameDBSites.API.Models.Cards
{
    public class CardFilterCollectionApiModel
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string Attribute { get; set; }
        public required CardFilterLandingApiModel[] Landings { get; set; }
    }
}
