using AdServer.Models;
using AdServer.Repositories.MetricDataRepository;
using SkytearHorde.Entities.Models.Database.AdServer;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories.AdServer
{
    public class MetricRawDataRepository : IMetricRawDataRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public MetricRawDataRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Add(AdMetricRawData data)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Insert(new MetricRawDataDBModel
            {
                AdId = data.AdId,
                Click = data.Click,
                Impression = data.Impression,
                Tracked = data.Tracked,
                InsertedUtc = DateTime.UtcNow
            });

            scope.Complete();
        }

        public AdMetricRawData[] GetOldest(int amount)
        {
            using var scope = _scopeProvider.CreateScope();
            var entities = scope.Database.Fetch<MetricRawDataDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .SelectTop(amount)
                .From<MetricRawDataDBModel>()
                .OrderBy<MetricRawDataDBModel>(it => it.InsertedUtc))
                .ToArray();
            foreach(var entity in entities)
            {
                scope.Database.Delete(entity);
            }
            scope.Complete();
            return entities
                .Select(it => new AdMetricRawData
                {
                    AdId = it.AdId,
                    Click = it.Click,
                    Impression = it.Impression,
                    Tracked = it.Tracked
                }).ToArray();
        }
    }
}
