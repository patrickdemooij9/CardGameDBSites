using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services.Search
{
    public interface ICardSearchService
    {
        int[] Search(CardSearchQuery query, out int totalItems);
    }
}
