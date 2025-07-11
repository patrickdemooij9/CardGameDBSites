﻿namespace SkytearHorde.Business.Services.Search
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
        public int? VariantTypeId { get; set; }
        public bool IncludeHideFromDeck { get; set; } = true;

        public CardSearchQuery(int amount, int siteId)
        {
            Amount = amount;
            SiteId = siteId;

            FilterClauses = [];
            OrderBy = [];
        }
    }
}
