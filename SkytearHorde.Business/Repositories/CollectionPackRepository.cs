using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CollectionPackRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public CollectionPackRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public void AddPack(int setId, string content, int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Insert(new CollectionPackDBModel
            {
                SetId = setId,
                UserId = userId,
                SiteId = _siteAccessor.GetSiteId(),
                Content = content,
                CreatedDate = DateTime.UtcNow,
            });

            scope.Complete();
        }

        public int GetCount(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.FirstOrDefault<int>(scope.SqlContext.Sql()
                .SelectCount()
                .From<CollectionPackDBModel>()
                .Where<CollectionPackDBModel>(it => it.UserId == userId && it.SiteId == siteId));
        }
    }
}
