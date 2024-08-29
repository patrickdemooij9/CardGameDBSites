using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Processors;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class DeckTrackingSyncTask : RecurringHostedServiceBase
    {
        private readonly DeckTrackingProcessor _processor;

        private readonly Dictionary<int, int> _views;

        public DeckTrackingSyncTask(ILogger<DeckTrackingSyncTask> logger, DeckTrackingProcessor deckTrackingProcessor) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1))
        {
            _views = new Dictionary<int, int>();
            _processor = deckTrackingProcessor;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            _processor.Process();
            return Task.CompletedTask;
        }
    }
}
