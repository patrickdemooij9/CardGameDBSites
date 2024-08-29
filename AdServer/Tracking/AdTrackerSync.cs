using AdServer.Models;
using AdServer.Repositories.MetricDataRepository;
using AdServer.Repositories.MetricRepository;
using Microsoft.Extensions.Hosting;

namespace AdServer.Tracking
{
    public class AdTrackerSync : IHostedService, IDisposable
    {
        private readonly IMetricRawDataRepository _metricRawDataRepository;
        private readonly IMetricRepository _metricRepository;

        private Timer? _timer;

        public AdTrackerSync(IMetricRawDataRepository metricRawDataRepository, IMetricRepository metricRepository)
        {
            _metricRawDataRepository = metricRawDataRepository;
            _metricRepository = metricRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var itemsToProcess = _metricRawDataRepository.GetOldest(50);
            if (itemsToProcess.Length == 0) return;

            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            Dictionary<int, AdMetric> metricsToUpdate = new Dictionary<int, AdMetric>();
            foreach (var item in itemsToProcess)
            {
                if (!metricsToUpdate.ContainsKey(item.AdId))
                {
                    var newAdMetric = _metricRepository.Get(item.AdId, date);
                    newAdMetric ??= new AdMetric(item.AdId, date);

                    metricsToUpdate.Add(item.AdId, newAdMetric);
                }

                var adMetric = metricsToUpdate[item.AdId];
                if (item.Impression)
                {
                    adMetric.Impressions++;
                    if (item.Tracked)
                        adMetric.TrackedImpressions++;
                }
                else if (item.Click)
                {
                    adMetric.Clicks++;
                    if (item.Tracked)
                        adMetric.TrackedClicks++;
                }
            }

            _metricRepository.BulkUpdate(metricsToUpdate.Values.ToArray());
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
