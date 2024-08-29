using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Repository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Cache
{
    public class DeckRepositoryCachePolicy : DefaultRepositoryCachePolicy<Deck, int>
    {
        public DeckRepositoryCachePolicy(IAppPolicyCache cache, RepositoryCachePolicyOptions options) : base(cache, options)
        {
        }

        public Deck[] GetPaged(
            DeckPagedRequest request,
            Func<DeckPagedRequest, DeckPagedResult> performPagedGet,
            Func<int[]?, IEnumerable<Deck>> performGetAll,
            out int size)
        {
            var shouldBeCached = request.ShouldBeCached();
            if (shouldBeCached)
            {
                // Check if we have done this request before so that we can get the deck ids.
                var cacheResult = _cache.GetCacheItem<DeckPagedCached>($"{_entityTypeCacheKey}-GetPaged-{request.ToCacheKey()}");
                if (cacheResult != null)
                {
                    size = cacheResult.Total; //Should be with the cache
                    if (cacheResult.DeckIds.Length == 0)
                    {
                        return [];
                    }
                    return GetAll(cacheResult.DeckIds, performGetAll);
                }
            }

            var result = performPagedGet(request);
            var decks = result.Decks.ToArray();
            foreach (var deck in decks)
            {
                InsertEntity(GetEntityCacheKey(deck.Id), deck);
            }
            if (request.ShouldBeCached())
            {
                _cache.Insert($"{_entityTypeCacheKey}-GetPaged-{request.ToCacheKey()}",
                    () => new DeckPagedCached
                    {
                        DeckIds = decks.Select(it => it.Id).ToArray(),
                        Total = result.Total
                    },
                    TimeSpan.FromMinutes(5),
                    true);
            }
            size = result.Total;
            return decks;
        }

        protected override void ClearBaseCache()
        {
            base.ClearBaseCache();
            _cache.ClearByKey($"{_entityTypeCacheKey}-GetPaged");
        }
    }
}
