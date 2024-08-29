using AdServer;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class AdReportTask : RecurringHostedServiceBase
    {
        private readonly CampaignReportSender _campaignReportSender;
        private readonly IRuntimeState _runtimeState;

        public AdReportTask(ILogger<AdReportTask> logger, CampaignReportSender campaignReportSender, IRuntimeState runtimeState) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _campaignReportSender = campaignReportSender;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            _campaignReportSender.CheckAndSendReports();
            return Task.CompletedTask;
        }
    }
}
