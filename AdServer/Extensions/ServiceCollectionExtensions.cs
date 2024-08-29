using AdServer.Checks;
using AdServer.Finder;
using AdServer.Tracking;
using Microsoft.Extensions.DependencyInjection;

namespace AdServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAdServer(this IServiceCollection service)
        {
            service.AddScoped<AdFinder>();
            service.AddScoped<AdTracker>();
            service.AddSingleton<CampaignReportSender>();

            service.AddScoped<IRequestCheck, UserAgentCheck>();
            service.AddSingleton<IRequestCheck, TrottleIPCheck>();

            service.AddHostedService<AdTrackerSync>();
        }
    }
}
