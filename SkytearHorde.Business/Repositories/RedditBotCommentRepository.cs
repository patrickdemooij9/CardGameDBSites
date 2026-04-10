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

        public bool HasProcessedComment(string commentFullName)
        {
            using var scope = _scopeProvider.CreateScope();
            var siteId = _siteAccessor.GetSiteId();
            var entity = scope.Database.FirstOrDefault<RedditBotCommentDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedditBotCommentDBModel>()
                    .Where<RedditBotCommentDBModel>(it => it.CommentFullName == commentFullName && it.SiteId == siteId));
            return entity != null;
        }

        public void AddProcessedComment(string commentFullName)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = new RedditBotCommentDBModel
            {
                CommentFullName = commentFullName,
                SiteId = _siteAccessor.GetSiteId(),
                ProcessedAt = DateTime.UtcNow
            };
            scope.Database.Insert(entity);

            scope.Complete();
        }
    }
}
