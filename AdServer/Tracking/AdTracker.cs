using AdServer.Checks;
using AdServer.Enums;
using AdServer.Models;
using AdServer.Repositories.MetricDataRepository;
using Microsoft.AspNetCore.Http;

namespace AdServer.Tracking
{
    public class AdTracker
    {
        private readonly IMetricRawDataRepository _metricDataRepository;
        private readonly IEnumerable<IRequestCheck> _requestChecks;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdTracker(IMetricRawDataRepository metricDataRepository,
            IEnumerable<IRequestCheck> requestChecks,
            IHttpContextAccessor httpContextAccessor)
        {
            _metricDataRepository = metricDataRepository;
            _requestChecks = requestChecks;
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddImpression(int adId)
        {
            SendToDatabase(new AdMetricRawData
            {
                AdId = adId,
                Impression = true,
                Tracked = CheckTracked(TrackingSource.Impression)
            });
        }

        public void AddClick(int adId)
        {
            SendToDatabase(new AdMetricRawData { AdId = adId, Click = true, Tracked = CheckTracked(TrackingSource.Click) });
        }

        private void SendToDatabase(AdMetricRawData data)
        {
            _metricDataRepository.Add(data);
        }

        private bool CheckTracked(TrackingSource source)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return _requestChecks.All(it => it.Check(httpContext, source));
        }
    }
}
