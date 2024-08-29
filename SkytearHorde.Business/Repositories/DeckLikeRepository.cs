using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckLikeRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DeckLikeRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public int[] GetLikedDecks(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<DeckLikeDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckLikeDBModel>()
                .Where<DeckLikeDBModel>(it => it.UserId == userId)).Select(it => it.DeckId).ToArray();
        }

        public void AddLike(int userId, int deckId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(new DeckLikeDBModel { UserId = userId, DeckId = deckId });
            scope.Complete();
        }

        public void DeleteLike(int userId, int deckId)
        {
            using var scope = _scopeProvider.CreateScope();
            var like = scope.Database.FirstOrDefault<DeckLikeDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckLikeDBModel>()
                .Where<DeckLikeDBModel>(it => it.UserId == userId && it.DeckId == deckId));
            if (like is null) return;

            scope.Database.Delete(like);
            scope.Complete();
        }
    }
}
