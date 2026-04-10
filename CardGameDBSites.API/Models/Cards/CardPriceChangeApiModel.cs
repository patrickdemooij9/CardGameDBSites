namespace CardGameDBSites.API.Models.Cards
{
    public class CardPriceChangeApiModel
    {
        public required int CardId { get; set; }
        public required int? VariantId { get; set; }
        public required string CardName { get; set; }
        public required string UrlSegment { get; set; }
        public required double CurrentPrice { get; set; }
        public required double PreviousPrice { get; set; }
        public required double PriceChange { get; set; }
        public required double PriceChangePercent { get; set; }
    }
}
