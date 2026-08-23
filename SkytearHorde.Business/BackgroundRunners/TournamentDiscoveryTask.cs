using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Business.Tournaments;
using SkytearHorde.Entities.Models.Business.Config;
using SkytearHorde.Entities.Models.Business.Tournament;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    /// <summary>
    /// Discovers recently-finished tournaments per site/connector and enqueues the worthwhile ones for import,
    /// reusing the existing <see cref="TournamentService.QueueImport"/> → <see cref="TournamentImportQueueTask"/>
    /// path. Only enqueues; the heavy import still runs in the queue task. Configured via appsettings
    /// (<see cref="TournamentDiscoveryOptions"/>).
    /// </summary>
    public class TournamentDiscoveryTask : RecurringHostedServiceBase
    {
        private readonly ILogger<TournamentDiscoveryTask> _logger;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly TournamentRepository _tournamentRepository;
        private readonly TournamentImportQueueRepository _importQueueRepository;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRuntimeState _runtimeState;
        private readonly SettingsService _settingsService;

        public TournamentDiscoveryTask(
            ILogger<TournamentDiscoveryTask> logger,
            ISiteService siteService,
            ISiteAccessor siteAccessor,
            TournamentRepository tournamentRepository,
            TournamentImportQueueRepository importQueueRepository,
            IUmbracoContextFactory umbracoContextFactory,
            IServiceProvider serviceProvider,
            IRuntimeState runtimeState,
            SettingsService settingsService)
            : base(logger, TimeSpan.FromHours(2), TimeSpan.FromSeconds(30))
        {
            _logger = logger;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _tournamentRepository = tournamentRepository;
            _importQueueRepository = importQueueRepository;
            _umbracoContextFactory = umbracoContextFactory;
            _serviceProvider = serviceProvider;
            _runtimeState = runtimeState;
            _settingsService = settingsService;
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var siteId in _siteService.GetAllSites())
            {
                _siteAccessor.SetSiteId(siteId);

                var settings = _settingsService.GetTournamentSettings();
                if (settings is null) continue;

                try
                {
                    await DiscoverForSite(siteId, settings);
                }
                catch (Exception ex)
                {
                    // Error isolation: one site/connector failure must not abort discovery for the rest.
                    _logger.LogError(ex, "Tournament discovery failed for site {SiteId}", siteId);
                }
            }
        }

        private async Task DiscoverForSite(int siteId, TournamentSettingsConfig siteConfig)
        {
            using var scope = _serviceProvider.CreateScope();
            var discoveryServices = scope.ServiceProvider.GetServices<ITournamentDiscoverer>();

            foreach (var discoverySource in siteConfig.Discovery)
            {
                var discoverer = scope.ServiceProvider.GetServices<ITournamentDiscoverer>() .FirstOrDefault(d => d.Source == discoverySource);
                if (discoverer is null)
                {
                    _logger.LogWarning("No tournament discoverer registered for source {Source} (site {SiteId})", discoverySource, siteId);
                    return;
                }

                var tournamentService = scope.ServiceProvider.GetRequiredService<TournamentService>();

                var discovered = await discoverer.DiscoverTournaments(new TournamentDiscoveryConfig
                {
                    GameDescription = "StarWarsUnlimited",
                    MinPlayers = 12
                });

                var enqueued = 0;
                foreach (var tournament in discovered)
                {
                    if (enqueued >= 20) break;

                    // Only completed tournaments have final standings/decks; anything else fails to import.
                    if (!string.Equals(tournament.Status, "Ended", StringComparison.OrdinalIgnoreCase)) continue;
                    // Game filter is client-side (source mixes all games); re-assert as a safety net.
                    if (!string.Equals(tournament.GameDescription, "StarWarsUnlimited", StringComparison.OrdinalIgnoreCase)) continue;
                    if (tournament.EnrolledPlayerCount < 12) continue;

                    // Dedup: QueueImport re-syncs already-imported tournaments rather than skipping, so we must skip
                    // ones already in the DB or already queued, or every prior tournament re-imports every run.
                    if (_tournamentRepository.GetBySourceAndExternalId(siteId, discoverer.Source, tournament.ExternalId) is not null) continue;
                    if (_importQueueRepository.ExistsPending(siteId, discoverer.Source, tournament.ExternalId)) continue;

                    // Only worth importing when most entrants submitted a decklist (the one per-candidate HTTP cost).
                    var coverage = await discoverer.GetDecklistCoverage(tournament.ExternalId);
                    if (coverage is null || coverage.Percent < 50) continue;

                    var formatId = 1;
                    var result = tournamentService.QueueImport(new ImportTournament
                    {
                        FormatId = formatId,
                        Type = tournament.Type,
                        Source = discoverer.Source,
                        ExternalId = tournament.ExternalId
                    });

                    if (result.Success)
                    {
                        enqueued++;
                        _logger.LogInformation(
                            "Queued discovered tournament {Source}/{ExternalId} \"{Name}\" ({Players} players, {Coverage:0}% decklist coverage) for site {SiteId}",
                            discoverer.Source, tournament.ExternalId, tournament.Name, tournament.EnrolledPlayerCount, coverage.Percent, siteId);
                    }
                }

                if (enqueued > 0)
                    _logger.LogInformation("Tournament discovery queued {Count} import(s) for site {SiteId} ({Source})", enqueued, siteId, discoverer.Source);
            }
        }

        /*private static (int FormatId, string Type) ResolveFormat(TournamentDiscoverySiteConfig siteConfig, string? formatString)
        {
            if (!string.IsNullOrEmpty(formatString))
            {
                var mapping = siteConfig.FormatMappings
                    .FirstOrDefault(m => string.Equals(m.MeleeFormat, formatString, StringComparison.OrdinalIgnoreCase));
                if (mapping is not null)
                    return (mapping.FormatId, mapping.Type);
            }

            return (siteConfig.DefaultFormatId, siteConfig.DefaultType);
        }*/
    }
}
