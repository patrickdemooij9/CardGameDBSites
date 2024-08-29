using AdServer;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class AdReportTask : RecurringHostedServiceBase
    {
        private readonly CampaignReportSender _campaignReportSender;

        public AdReportTask(ILogger<AdReportTask> logger, CampaignReportSender campaignReportSender) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _campaignReportSender = campaignReportSender;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            _campaignReportSender.CheckAndSendReports();
            return Task.CompletedTask;
        }
    }
}
