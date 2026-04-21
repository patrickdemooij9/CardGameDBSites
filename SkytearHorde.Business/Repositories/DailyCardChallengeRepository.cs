using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Database.DailyGame;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DailyCardChallengeRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public DailyCardChallengeRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public DailyCardChallengeDBModel? GetByDate(DateTime challengeDateUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var siteId = _siteAccessor.GetSiteId();
            var date = challengeDateUtc.Date;

            return scope.Database.FirstOrDefault<DailyCardChallengeDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DailyCardChallengeDBModel>()
                .Where<DailyCardChallengeDBModel>(it => it.SiteId == siteId && it.ChallengeDateUtc == date));
        }

        public DailyCardChallengeDBModel Add(int targetCardId, DateTime challengeDateUtc)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = new DailyCardChallengeDBModel
            {
                SiteId = _siteAccessor.GetSiteId(),
                ChallengeDateUtc = challengeDateUtc.Date,
                TargetCardId = targetCardId,
                CreatedUtc = DateTime.UtcNow
            };
            scope.Database.Insert(entity);
            scope.Complete();
            return entity;
        }
    }
}
