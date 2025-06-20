using SkytearHorde.Business.Services.Search;

namespace CardGameDBSites.API.Models.Cards
{
    public class CardsQueryFilterClauseApiModel
    {
        public CardsQueryFilterApiModel[] Filters { get; set; } = [];
        public CardSearchFilterClauseType ClauseType { get; set; }
    }
}
