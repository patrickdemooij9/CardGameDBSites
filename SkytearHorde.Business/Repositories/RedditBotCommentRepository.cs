using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class RedditBotCommentRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public RedditBotCommentRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public DateTime? GetLastProcessedDate()
        {
            using var scope = _scopeProvider.CreateScope();
            var siteId = _siteAccessor.GetSiteId();
            var entity = scope.Database.FirstOrDefault<RedditBotCommentDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedditBotCommentDBModel>()
                    .Where<RedditBotCommentDBModel>(it => it.SiteId == siteId));
            return entity?.LastProcessedAt;
        }

        public void UpdateLastProcessedDate(DateTime lastProcessedAt)
        {
            using var scope = _scopeProvider.CreateScope();
            var siteId = _siteAccessor.GetSiteId();

            var existing = scope.Database.FirstOrDefault<RedditBotCommentDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedditBotCommentDBModel>()
                    .Where<RedditBotCommentDBModel>(it => it.SiteId == siteId));

            if (existing != null)
            {
                existing.LastProcessedAt = lastProcessedAt;
                scope.Database.Update(existing);
            }
            else
            {
                scope.Database.Insert(new RedditBotCommentDBModel
                {
                    SiteId = siteId,
                    LastProcessedAt = lastProcessedAt
                });
            }

            scope.Complete();
        }
    }
}
