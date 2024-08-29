using AdServer.Models;
using AdServer.Repositories.AdRepository;
using AdServer.Repositories.MetricRepository;
using SkytearHorde.Entities.Models.Database.AdServer;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SkytearHorde.Business.Repositories.AdServer
{
    public class MetricRepository : IMetricRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public MetricRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void BulkUpdate(AdMetric[] metrics)
        {
            using var scope = _scopeProvider.CreateScope();
            foreach(var item in metrics)
            {
                var entity = GetEntity(scope, item.AdId, item.Date) ?? new MetricDBModel();

                entity.AdId = item.AdId;
                entity.Date = item.Date.ToDateTime(TimeOnly.MinValue);
                entity.Clicks = item.Clicks;
                entity.Impressions = item.Impressions;
                entity.TrackedClicks = item.TrackedClicks;
                entity.TrackedImpressions = item.TrackedImpressions;

                scope.Database.Save(entity);
            }
            scope.Complete();
        }

        public AdMetric? Get(int adId, DateOnly date)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = GetEntity(scope, adId, date);
            if (entity is null) return null;
            return new AdMetric(adId, date)
            {
                Clicks = entity.Clicks,
                Impressions = entity.Impressions,
                TrackedClicks = entity.TrackedClicks,
                TrackedImpressions = entity.TrackedImpressions
            };
        }

        public AdMetric[] GetByAds(int[] adIds)
        {
            using var scope = _scopeProvider.CreateScope();
            var metrics = scope.Database.Fetch<MetricDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<MetricDBModel>()
                .Where<MetricDBModel>(it => adIds.Contains(it.AdId)))
                .Select(entity => new AdMetric(entity.AdId, DateOnly.FromDateTime(entity.Date))
                {
                    Clicks = entity.Clicks,
                    Impressions = entity.Impressions,
                    TrackedClicks = entity.TrackedClicks,
                    TrackedImpressions = entity.TrackedImpressions
                }).ToArray();
            return metrics;
        }

        private MetricDBModel? GetEntity(IScope scope, int adId, DateOnly date)
        {
            var compareDate = date.ToDateTime(TimeOnly.MinValue);
            return scope.Database.FirstOrDefault<MetricDBModel>(scope.SqlContext.Sql()
            .SelectAll()
                .From<MetricDBModel>()
                .Where<MetricDBModel>(it => it.AdId == adId && it.Date == compareDate));
        }
    }
}
