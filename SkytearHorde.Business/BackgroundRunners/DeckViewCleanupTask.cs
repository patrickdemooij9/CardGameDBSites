using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Repositories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class DeckViewCleanupTask : RecurringHostedServiceBase
    {
        private readonly DeckViewRepository _deckViewRepository;
        private readonly IRuntimeState _runtimeState;

        public DeckViewCleanupTask(ILogger<DeckViewCleanupTask> logger,
            DeckViewRepository deckViewRepository,
            IRuntimeState runtimeState) : base(logger, TimeSpan.FromHours(24), TimeSpan.FromMinutes(5))
        {
            _deckViewRepository = deckViewRepository;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            _deckViewRepository.CleanupOldViews();
            return Task.CompletedTask;
        }
    }
}
