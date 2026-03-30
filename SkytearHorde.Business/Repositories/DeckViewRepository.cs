using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckViewRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DeckViewRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void AddViews(int deckId, int views)
        {
            var currentDate = DateTime.UtcNow.Date;
            using var scope = _scopeProvider.CreateScope();

            var existingModel = scope.Database.FirstOrDefault<DeckViewDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckViewDBModel>()
                .Where<DeckViewDBModel>(it => it.DeckId == deckId && it.Date == currentDate));

            if (existingModel is null)
            {
                existingModel = new DeckViewDBModel
                {
                    DeckId = deckId,
                    Date = currentDate
                };
            }
            existingModel.Views += views;
            scope.Database.Save(existingModel);

            scope.Database.Execute("UPDATE Deck SET TotalViews = TotalViews + @0 WHERE Id = @1", views, deckId);

            scope.Complete();
        }

        public int[] GetLast7Days(int deckId)
        {
            var date = DateTime.UtcNow.AddDays(-7);
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<int>(scope.SqlContext.Sql()
                .Select("Views")
                .From<DeckViewDBModel>()
                .Where<DeckViewDBModel>(it => it.DeckId == deckId && date <= it.Date)).Take(7).ToArray();
        }

        public void CleanupOldViews()
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM DeckView WHERE Date < DATEADD(day, -30, GETUTCDATE())");
            scope.Complete();
        }
    }
}
