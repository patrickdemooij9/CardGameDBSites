using AdServer.Models;

namespace AdServer.Repositories.MetricDataRepository
{
    public interface IMetricRawDataRepository
    {
        void Add(AdMetricRawData data);
        AdMetricRawData[] GetOldest(int amount);
    }
}
