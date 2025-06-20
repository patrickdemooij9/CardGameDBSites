namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchFilterClause
    {
        public CardSearchFilter[] Filters { get; set; } = [];
        public CardSearchFilterClauseType ClauseType { get; set; }
    }
}
