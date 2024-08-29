using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Processors;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class DeckTrackingSyncTask : RecurringHostedServiceBase
    {
        private readonly DeckTrackingProcessor _processor;
        private readonly IRuntimeState _runtimeState;

        public DeckTrackingSyncTask(ILogger<DeckTrackingSyncTask> logger, DeckTrackingProcessor deckTrackingProcessor, IRuntimeState runtimeState) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _processor = deckTrackingProcessor;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            _processor.Process();
            return Task.CompletedTask;
        }
    }
}
