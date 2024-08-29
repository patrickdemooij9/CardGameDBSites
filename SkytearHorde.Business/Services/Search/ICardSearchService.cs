using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services.Search
{
    public interface ICardSearchService
    {
        Card[] Search(CardSearchQuery query);
    }
}
