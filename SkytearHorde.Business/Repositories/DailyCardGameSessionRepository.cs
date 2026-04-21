using SkytearHorde.Entities.Models.Database.DailyGame;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SkytearHorde.Business.Repositories
{
    public class DailyCardGameSessionRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DailyCardGameSessionRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public DailyCardGameSessionDBModel? GetByChallengeAndMember(int challengeId, int memberId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<DailyCardGameSessionDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DailyCardGameSessionDBModel>()
                .Where<DailyCardGameSessionDBModel>(it => it.ChallengeId == challengeId && it.MemberId == memberId));
        }

        public DailyCardGameSessionDBModel Add(DailyCardGameSessionDBModel session)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(session);
            scope.Complete();
            return session;
        }

        public void Save(DailyCardGameSessionDBModel session)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Update(session);
            scope.Complete();
        }

        public DailyCardGameSessionDBModel[] GetCompletedByChallenge(int challengeId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<DailyCardGameSessionDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DailyCardGameSessionDBModel>()
                .Where<DailyCardGameSessionDBModel>(it => it.ChallengeId == challengeId && it.Status != "InProgress")).ToArray();
        }
    }
}
