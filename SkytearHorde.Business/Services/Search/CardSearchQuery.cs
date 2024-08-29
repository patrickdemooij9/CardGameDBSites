namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchQuery
    {
        public string? Query { get; set; }
        public Dictionary<string, string[]> CustomFields { get; set; }
        public int Amount { get; set; }
        public int SiteId { get; set; }
        public int? SetId { get; set; }
        public string? SortBy { get; set; }
        public int? VariantTypeId { get; set; }
        public bool SortDescending { get; set; }
        public bool IncludeHideFromDeck { get; set; } = true;

        public CardSearchQuery(int amount, int siteId)
        {
            Amount = amount;
            SiteId = siteId;

            CustomFields = new Dictionary<string, string[]>();
        }
    }
}
