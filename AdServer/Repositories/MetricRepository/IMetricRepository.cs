using AdServer.Models;

namespace AdServer.Repositories.MetricRepository
{
    public interface IMetricRepository
    {
        void BulkUpdate(AdMetric[] metrics);
        AdMetric? Get(int adId, DateOnly date);
        AdMetric[] GetByAds(int[] adIds);
    }
}
