using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CollectionCardRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IAppPolicyCache _cache;

        public CollectionCardRepository(IScopeProvider scopeProvider, AppCaches appCaches)
        {
            _scopeProvider = scopeProvider;
            _cache = appCaches.RuntimeCache;
        }

        public void Add(CollectionCardItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Insert(new CollectionCardDBModel
            {
                CardId = item.CardId,
                VariantId = item.VariantId,
                UserId = item.UserId,
                Amount = item.Amount
            });
            scope.Complete();

            ClearCache(item.UserId);
        }

        public void Update(CollectionCardItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Update(new CollectionCardDBModel
            {
                Id = item.Id,
                CardId = item.CardId,
                VariantId = item.VariantId,
                UserId = item.UserId,
                Amount = item.Amount
            });
            scope.Complete();

            ClearCache(item.UserId);
        }

        public CollectionCardItem[] Get(int cardId, int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entities = scope.Database.Query<CollectionCardDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CollectionCardDBModel>()
                .Where<CollectionCardDBModel>(it => it.CardId == cardId && it.UserId == userId));
            if (entities is null) return Array.Empty<CollectionCardItem>();

            return entities.Select(Map).ToArray();
        }

        public CollectionCardItem? Get(int cardId, int variantId, int userId)
        {
            using var scope = _scopeProvider.CreateScope();
            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<CollectionCardDBModel>()
                .Where<CollectionCardDBModel>(it => it.CardId == cardId && it.UserId == userId && it.VariantId == variantId);

            var entity = scope.Database.FirstOrDefault<CollectionCardDBModel>(sql);
            if (entity is null) return null;

            return Map(entity);
        }

        public CollectionCardItem[] Get(int userId)
        {
            return _cache.GetCacheItem($"CollectionCard_Get_{userId}", () =>
            {
                using var scope = _scopeProvider.CreateScope();

                var entities = scope.Database.Query<CollectionCardDBModel>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<CollectionCardDBModel>()
                    .Where<CollectionCardDBModel>(it => it.UserId == userId));
                if (entities is null) return Array.Empty<CollectionCardItem>();

                return entities.Select(Map).ToArray();
            }, TimeSpan.FromMinutes(5))!;
        }

        public void Remove(CollectionCardItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete<CollectionCardDBModel>(item.Id);
            scope.Complete();

            ClearCache(item.UserId);
        }

        public void Remove(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var items = scope.Database.Fetch<CollectionCardDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CollectionCardDBModel>()
                .Where<CollectionCardDBModel>(it => it.UserId == userId));
            foreach (var item in items)
            {
                scope.Database.Delete(item);
            }
            scope.Complete();

            ClearCache(userId);
        }

        private CollectionCardItem Map(CollectionCardDBModel entity)
        {
            return new CollectionCardItem
            {
                Id = entity.Id,
                CardId = entity.CardId,
                VariantId = entity.VariantId!.Value,
                UserId = entity.UserId,
                Amount = entity.Amount
            };
        }

        private void ClearCache(int userId)
        {
            _cache.ClearByKey($"CollectionCard_Get_{userId}");
        }
    }
}
