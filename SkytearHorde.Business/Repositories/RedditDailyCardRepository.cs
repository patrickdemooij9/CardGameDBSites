using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class RedditDailyCardRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public RedditDailyCardRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public int[] GetCards()
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.Fetch<int>(scope.SqlContext.Sql()
                .Select<RedditDailyCardDBModel>(it => it.CardId)
                .From<RedditDailyCardDBModel>()
                .Where<RedditDailyCardDBModel>(it => it.SiteId == siteId)).ToArray();
        }

        public void AddCard(int cardId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = new RedditDailyCardDBModel
            {
                CardId = cardId,
                SiteId = _siteAccessor.GetSiteId(),
                Date = DateTime.UtcNow.Date
            };
            scope.Database.Insert(entity);

            scope.Complete();
        }
    }
}
