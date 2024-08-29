using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CollectionSetRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public CollectionSetRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Add(CollectionSetItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Insert(new CollectionSetDBModel
            {
                SetId = item.SetId,
                UserId = item.UserId,
                Amount = item.Amount
            });
            scope.Complete();
        }

        public CollectionSetItem? Get(int setId, int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = scope.Database.FirstOrDefault<CollectionSetDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CollectionSetDBModel>()
                .Where<CollectionSetDBModel>(it => it.SetId == setId && it.UserId == userId));
            if (entity is null) return null;

            return Map(entity);
        }

        public CollectionSetItem[] Get(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entities = scope.Database.Query<CollectionSetDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CollectionSetDBModel>()
                .Where<CollectionSetDBModel>(it => it.UserId == userId));
            if (entities is null) return Array.Empty<CollectionSetItem>();

            return entities.Select(Map).ToArray();
        }

        public void Remove(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete<CollectionSetDBModel>(id);
            scope.Complete();
        }

        private CollectionSetItem Map(CollectionSetDBModel entity)
        {
            return new CollectionSetItem
            {
                Id = entity.Id,
                SetId = entity.SetId,
                UserId = entity.UserId,
                Amount = entity.Amount
            };
        }
    }
}
