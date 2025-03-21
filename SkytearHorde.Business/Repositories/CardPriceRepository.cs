﻿using SkytearHorde.Entities.Models.Business;
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

        private readonly Cache.IRepositoryCachePolicy<CardPriceGroup, int> _repositoryCachePolicy;

        public CardPriceRepository(IScopeProvider scopeProvider, IAppPolicyCache cache)
        {
            _scopeProvider = scopeProvider;
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

            using var scope = _scopeProvider.CreateScope();
            foreach (var record in prices)
            {
                latestRecords.TryGetValue(record.CardId, out var lastRecords);

                foreach (var price in record.Prices)
                {
                    var existingRecord = lastRecords?.FirstOrDefault(it => it.VariantId == price.VariantId);
                    if (existingRecord != null)
                    {
                        if (existingRecord.DateUtc == price.DateUtc)
                        {
                            existingRecord.MainPrice = price.MainPrice;
                            existingRecord.LowestPrice = price.LowestPrice;
                            existingRecord.HighestPrice = price.HighestPrice;
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
                    };
                    scope.Database.Insert(newRecord);

                    _repositoryCachePolicy.ClearCache(record.CardId);
                }
            }

            scope.Complete();
        }

        private int PerformCount()
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.First<int>("select count(*) from (select distinct CardId, VariantId from CardPriceRecord group by CardId, VariantId) c");
        }

        private List<CardPriceRecordDBModel> GetRecords(int[] ids)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.Fetch<CardPriceRecordDBModel>(scope.SqlContext.Sql("" +
                "SELECT r.* FROM CardPriceRecord r INNER JOIN (SELECT CardId, MAX(DateUtc) as 'MaxDate' FROM CardPriceRecord GROUP BY CardId) r2 on r.CardId = r2.CardId AND r.DateUtc = r2.MaxDate WHERE r.CardId in (@0)", ids));
        }
    }
}
