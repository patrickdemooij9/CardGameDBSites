using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using System.Security.Cryptography;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CardPriceRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IAppPolicyCache _cache;

        private readonly Cache.IRepositoryCachePolicy<CardPriceGroup, int> _repositoryCachePolicy;

        public CardPriceRepository(IScopeProvider scopeProvider, IAppPolicyCache cache)
        {
            _scopeProvider = scopeProvider;
            _cache = cache;
            _repositoryCachePolicy = new Cache.DefaultRepositoryCachePolicy<CardPriceGroup, int>(cache, new Cache.RepositoryCachePolicyOptions
            {
                PerformCount = PerformCount
            });
        }

        public CardPriceGroup[] GetPrices(params int[] ids)
        {
            //var test = _repositoryCachePolicy.GetAll(ids, DoGet).ToArray();
            return _repositoryCachePolicy.GetAll(ids, DoGet);
        }

        private CardPriceGroup[] DoGet(params int[]? ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var result = new CardPriceGroup[ids.Length];
            var records = GetRecords(ids).GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var cardPriceGroup = new CardPriceGroup
                {
                    CardId = id,
                    SourceId = 0
                };

                if (records.TryGetValue(id, out var prices))
                {
                    cardPriceGroup.Prices.AddRange(prices.Select(p => new CardPrice
                    {
                        VariantId = p.VariantId,
                        HighestPrice = p.HighestPrice,
                        LowestPrice = p.LowestPrice,
                        MainPrice = p.MainPrice,
                        DateUtc = p.DateUtc,
                    }));
                }
                result[i] = cardPriceGroup;
            }
            return result;
        }

        public void InsertPrices(CardPriceGroup[] prices)
        {
            var latestRecords = GetRecords(prices.Select(it => it.CardId).ToArray()).GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);

            foreach (var record in prices)
            {
                latestRecords.TryGetValue(record.CardId, out var lastRecords);

                foreach (var price in record.Prices)
                {
                    using var scope = _scopeProvider.CreateScope(autoComplete: true);
                    var existingRecord = lastRecords?.FirstOrDefault(it => it.VariantId == price.VariantId);
                    if (existingRecord != null)
                    {
                        if (existingRecord.DateUtc == price.DateUtc)
                        {
                            existingRecord.MainPrice = price.MainPrice;
                            existingRecord.LowestPrice = price.LowestPrice;
                            existingRecord.HighestPrice = price.HighestPrice;
                            existingRecord.Delta = CardPriceDeltaCalculator.RecalculateDelta(price.MainPrice, existingRecord);
                            scope.Database.Update(existingRecord);
                            continue;
                        }

                        if (existingRecord.MainPrice == price.MainPrice
                            && existingRecord.LowestPrice == price.LowestPrice
                            && existingRecord.HighestPrice == price.HighestPrice)
                            continue;
                    }

                    var newRecord = new CardPriceRecordDBModel
                    {
                        SourceId = record.SourceId,
                        CardId = record.CardId,
                        VariantId = price.VariantId,
                        HighestPrice = price.HighestPrice,
                        LowestPrice = price.LowestPrice,
                        MainPrice = price.MainPrice,
                        DateUtc = price.DateUtc,
                        IsLatest = true,
                        Delta = CardPriceDeltaCalculator.CalculateDelta(price.MainPrice, existingRecord)
                    };
                    scope.Database.Insert(newRecord);

                    if (existingRecord != null)
                    {
                        existingRecord.IsLatest = false;
                        scope.Database.Update(existingRecord);
                    }

                    _repositoryCachePolicy.ClearCache(record.CardId);
                }
            }
        }
        public List<CardPriceChangeResult> GetPriceChanges(bool descending)
        {
            var cacheKey = $"uRepo_PriceChanges_{descending}";
            var cached = _cache.GetCacheItem<List<CardPriceChangeResult>>(cacheKey);
            if (cached != null)
                return cached;

            using var scope = _scopeProvider.CreateScope();
            var orderDirection = descending ? "DESC" : "ASC";
            var cutoffDate = DateTime.UtcNow.Date.AddDays(-1);
            var sql = $@"
                SELECT CardId, VariantId, MainPrice AS CurrentPrice, (MainPrice - Delta) AS PreviousPrice
                FROM CardPriceRecord
                WHERE IsLatest = 1 AND Delta <> 0 AND (MainPrice - Delta) > 0 AND DateUtc >= @0
                ORDER BY Delta {orderDirection}";
            var result = scope.Database.Fetch<CardPriceChangeResult>(sql, cutoffDate);
            _cache.Insert(cacheKey, () => result, TimeSpan.FromHours(1));
            return result;
        }

        /// <summary>
        /// Weekly movers: compares each variant's latest price to the most recent price recorded
        /// on or before 7 days before the latest sync. Anchoring on the latest recorded date (rather
        /// than "now") keeps it correct on Sunday when the sync is fresh and robust if the sync lags.
        /// Ordered by absolute change (descending = risers, ascending = fallers).
        /// </summary>
        public List<CardPriceChangeResult> GetWeeklyPriceChanges(bool descending)
        {
            var cacheKey = $"uRepo_WeeklyPriceChanges_{descending}";
            var cached = _cache.GetCacheItem<List<CardPriceChangeResult>>(cacheKey);
            if (cached != null)
                return cached;

            using var scope = _scopeProvider.CreateScope();
            var orderDirection = descending ? "DESC" : "ASC";
            // Single window-function pass over the pre-cutoff rows, joined to the current (IsLatest)
            // rows. Derived tables (not CTEs) keep NPoco happy and avoid a per-card correlated scan.
            // TOP(@0) keeps the caller from having to resolve every changed card (only the movers).
            var sql = $@"
                SELECT l.CardId, l.VariantId, l.MainPrice AS CurrentPrice, w.PreviousPrice
                FROM CardPriceRecord l
                INNER JOIN (
                    SELECT CardId, VariantId, MainPrice AS PreviousPrice
                    FROM (
                        SELECT CardId, VariantId, MainPrice,
                               ROW_NUMBER() OVER (PARTITION BY CardId, VariantId ORDER BY DateUtc DESC, Id DESC) AS rn
                        FROM CardPriceRecord
                        WHERE DateUtc <= DATEADD(day, -7, (SELECT MAX(DateUtc) FROM CardPriceRecord))
                    ) ranked
                    WHERE ranked.rn = 1
                ) w ON w.CardId = l.CardId AND w.VariantId = l.VariantId
                WHERE l.IsLatest = 1 AND w.PreviousPrice > 0 AND l.MainPrice <> w.PreviousPrice
                ORDER BY (l.MainPrice - w.PreviousPrice) {orderDirection}";
            var result = scope.Database.Fetch<CardPriceChangeResult>(sql);
            _cache.Insert(cacheKey, () => result, TimeSpan.FromHours(1));
            return result;
        }

        /// <summary>The most recent price record date (the anchor for the weekly window). Null if there are no records.</summary>
        public DateTime? GetLatestPriceDate()
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.ExecuteScalar<DateTime?>("SELECT MAX(DateUtc) FROM CardPriceRecord");
        }

        public List<CardPriceRecordDBModel> GetPriceHistory(int cardId, int? variantId)
        {
            using var scope = _scopeProvider.CreateScope();
            const string columns = "Id, CardId, VariantId, MainPrice, LowestPrice, HighestPrice, DateUtc, IsLatest, Delta";
            if (variantId.HasValue)
            {
                return scope.Database.Fetch<CardPriceRecordDBModel>(
                    $"SELECT {columns} FROM CardPriceRecord WHERE CardId = @0 AND VariantId = @1 ORDER BY DateUtc ASC, Id ASC",
                    cardId, variantId.Value);
            }
            return scope.Database.Fetch<CardPriceRecordDBModel>(
                $"SELECT {columns} FROM CardPriceRecord WHERE CardId = @0 ORDER BY DateUtc ASC, Id ASC",
                cardId);
        }

        private int PerformCount()
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.First<int>("select count(*) from (select distinct CardId, VariantId from CardPriceRecord group by CardId, VariantId) c");
        }

        private List<CardPriceRecordDBModel> GetRecords(int[] ids)
        {
            using var scope = _scopeProvider.CreateScope();
            if (ids.Length == 0) return [];
            return [.. scope.Database.Fetch<CardPriceRecordDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardPriceRecordDBModel>()
                .Where<CardPriceRecordDBModel>(it => ids.Contains(it.CardId) && it.IsLatest)
            )];
        }
    }
}
