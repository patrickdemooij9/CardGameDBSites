using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Integrations.MeleeGg;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    /// <summary>
    /// Recurring background task that pulls completed SW Unlimited tournaments
    /// from melee.gg and imports them into the local tournament/meta system.
    /// Runs every 4 hours; can be disabled via <c>CardGameSettings:MeleeGg:Enabled</c>.
    /// </summary>
    public class MeleeSyncTask : RecurringHostedServiceBase
    {
        private readonly ILogger<MeleeSyncTask> _logger;
        private readonly MeleeGgTournamentSyncService _syncService;
        private readonly CardGameSettingsConfig _config;
        private readonly IRuntimeState _runtimeState;

        public MeleeSyncTask(
            ILogger<MeleeSyncTask> logger,
            MeleeGgTournamentSyncService syncService,
            IOptions<CardGameSettingsConfig> config,
            IRuntimeState runtimeState)
            : base(logger, TimeSpan.FromHours(4), TimeSpan.FromMinutes(2))
        {
            _logger = logger;
            _syncService = syncService;
            _config = config.Value;
            _runtimeState = runtimeState;
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return;

            var meleeConfig = _config.MeleeGg;
            if (!meleeConfig.Enabled)
            {
                _logger.LogDebug("melee.gg sync is disabled — skipping");
                return;
            }

            try
            {
                await _syncService.SyncAsync(meleeConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in melee.gg sync task");
            }
        }
    }
}
