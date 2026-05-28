namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchQuery
    {
        public string? Query { get; set; }
        public List<CardSearchFilterClause> FilterClauses { get; set; }
        public int Amount { get; set; }
        public int Skip { get; set; }
        public int SiteId { get; set; }
        public int? SetId { get; set; }
        public List<CardSorting> OrderBy { get; set; }
        public int[] VariantTypeIds { get; set; }
        public bool IncludeHideFromDeck { get; set; } = true;
        public CardSearchCollectionMode CollectionMode { get; set; }
        public int? MemberId { get; set; }
        public bool IncludeReprintedCards { get; set; } = true;
        public int? LegalForDeckTypeId { get; set; }

        public CardSearchQuery(int amount, int siteId)
        {
            Amount = amount;
            SiteId = siteId;

            FilterClauses = [];
            OrderBy = [];
            VariantTypeIds = [];

            CollectionMode = CardSearchCollectionMode.Ignore;
        }
    }
}
