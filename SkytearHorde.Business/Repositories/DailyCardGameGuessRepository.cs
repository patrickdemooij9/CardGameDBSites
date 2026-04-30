using SkytearHorde.Entities.Models.Database.DailyGame;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DailyCardGameGuessRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DailyCardGameGuessRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public DailyCardGameGuessDBModel[] GetBySession(int sessionId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<DailyCardGameGuessDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DailyCardGameGuessDBModel>()
                .Where<DailyCardGameGuessDBModel>(it => it.SessionId == sessionId)
                .OrderBy<DailyCardGameGuessDBModel>(it => it.AttemptNumber)).ToArray();
        }

        public void Add(DailyCardGameGuessDBModel guess)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(guess);
            scope.Complete();
        }
    }
}
