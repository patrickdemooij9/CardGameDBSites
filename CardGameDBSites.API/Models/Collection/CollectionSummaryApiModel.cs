namespace CardGameDBSites.API.Models.Collection
{
    public class CollectionSummaryApiModel
    {
        public int UniqueCards { get; set; }
        public int TotalCards { get; set; }
        public int PacksOpened { get; set; }
        public double MarketPrice { get; set; }
    }
}
