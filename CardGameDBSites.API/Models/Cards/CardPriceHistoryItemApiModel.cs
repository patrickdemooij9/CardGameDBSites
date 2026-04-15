namespace CardGameDBSites.API.Models.Cards
{
    public class CardPriceHistoryItemApiModel
    {
        public required string Date { get; set; }
        public required double Price { get; set; }
    }
}
