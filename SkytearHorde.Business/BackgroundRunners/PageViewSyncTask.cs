using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Processors;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class PageViewSyncTask : RecurringHostedServiceBase
    {
        private readonly ViewSessionGenerator _viewSessionGenerator;
        private readonly ViewTrackingProcessor _processor;

        public PageViewSyncTask(ILogger<DeckTrackingSyncTask> logger, ViewSessionGenerator viewSessionGenerator, ViewTrackingProcessor viewTrackingProcessor) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _viewSessionGenerator = viewSessionGenerator;
            _processor = viewTrackingProcessor;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            _viewSessionGenerator.GenerateSalt();
            _processor.SyncViews();
            return Task.CompletedTask;
        }
    }
}
