using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Integrations.MeleeGg
{
    /// <summary>
    /// Orchestrates syncing melee.gg tournament data into the local
    /// tournament/meta system.  Called by <c>MeleeSyncTask</c>.
    /// </summary>
    public class MeleeGgTournamentSyncService
    {
        private readonly MeleeGgApiClient _apiClient;
        private readonly TournamentRepository _tournamentRepository;
        private readonly TournamentEntrantRepository _entrantRepository;
        private readonly ILogger<MeleeGgTournamentSyncService> _logger;

        public MeleeGgTournamentSyncService(
            MeleeGgApiClient apiClient,
            TournamentRepository tournamentRepository,
            TournamentEntrantRepository entrantRepository,
            ILogger<MeleeGgTournamentSyncService> logger)
        {
            _apiClient = apiClient;
            _tournamentRepository = tournamentRepository;
            _entrantRepository = entrantRepository;
            _logger = logger;
        }

        public async Task SyncAsync(MeleeGgConfig config, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow.AddDays(-config.LookbackDays);

            _logger.LogInformation(
                "melee.gg sync started — game={Game}, siteId={Site}, formatId={Format}, since={Since:yyyy-MM-dd}",
                config.GameSlug, config.SiteId, config.FormatId, since);

            var tournaments = await _apiClient.GetCompletedTournamentsAsync(config, since, ct);
            _logger.LogInformation("melee.gg returned {Count} completed tournament(s)", tournaments.Count);

            int imported = 0;
            int skipped = 0;

            foreach (var summary in tournaments)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    await SyncTournamentAsync(config, summary, ct);
                    imported++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing tournament {Id} ({Name})", summary.Id, summary.Name);
                    skipped++;
                }
            }

            _logger.LogInformation(
                "melee.gg sync finished — imported={Imported}, skipped={Skipped}",
                imported, skipped);
        }

        private async Task SyncTournamentAsync(
            MeleeGgConfig config,
            MeleeGgTournamentSummary summary,
            CancellationToken ct)
        {
            // Check if we already have this tournament
            var existing = _tournamentRepository.GetByExternalId(summary.Id);
            Guid tournamentId;

            if (existing is not null)
            {
                tournamentId = existing.Id;
                _logger.LogDebug("Tournament {ExternalId} already exists (id={Id}), syncing entrants only",
                    summary.Id, tournamentId);
            }
            else
            {
                var tournamentSourceUrl = $"https://melee.gg/Tournament/View/{summary.Id}";
                var newTournament = new TournamentEvent
                {
                    Id = Guid.NewGuid(),
                    SiteId = config.SiteId,
                    Name = summary.Name,
                    Date = summary.StartDate,
                    FormatId = config.FormatId,
                    PlayerCount = summary.PlayerCount,
                    SourceUrl = tournamentSourceUrl,
                    ExternalId = summary.Id
                };

                tournamentId = _tournamentRepository.Create(newTournament);
                _logger.LogInformation("Created tournament {Name} (id={Id}) from melee.gg {ExternalId}",
                    summary.Name, tournamentId, summary.Id);
            }

            // Sync registrations
            var registrations = await _apiClient.GetRegistrationsAsync(config, summary.Id, ct);
            if (registrations.Count == 0)
            {
                _logger.LogDebug("No registrations found for tournament {ExternalId}", summary.Id);
                return;
            }

            var newEntrants = new List<TournamentEntrant>();

            foreach (var reg in registrations)
            {
                if (string.IsNullOrWhiteSpace(reg.Id)) continue;

                // Skip entrants we already imported
                if (_entrantRepository.ExternalIdExists(tournamentId, reg.Id)) continue;

                var playerName = reg.Player?.Name ?? "Unknown";
                newEntrants.Add(new TournamentEntrant
                {
                    Id = Guid.NewGuid(),
                    TournamentEventId = tournamentId,
                    PlayerName = playerName,
                    Placement = reg.Standing?.Placement,
                    Wins = reg.Standing?.Wins,
                    Losses = reg.Standing?.Losses,
                    Draws = reg.Standing?.Draws,
                    ExternalId = reg.Id,
                    // DeckId is left null — deck linking can be done separately
                    // once melee.gg decklist IDs are matched to local decks.
                });
            }

            if (newEntrants.Count > 0)
            {
                _entrantRepository.AddRange(newEntrants);
                _logger.LogInformation(
                    "Added {Count} entrant(s) to tournament {Name} ({ExternalId})",
                    newEntrants.Count, summary.Name, summary.Id);
            }
        }
    }
}
