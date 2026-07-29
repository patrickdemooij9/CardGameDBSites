using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    /// <summary>
    /// Processes queued tournament imports outside the HTTP request. The admin endpoint only enqueues
    /// a row; the actual (slow, external-HTTP-heavy) import runs here so it is immune to the Cloudflare
    /// request timeout. One row is processed at a time; the outcome is stored back on the row and logged.
    /// </summary>
    public class TournamentImportQueueTask : RecurringHostedServiceBase
    {
        private readonly ILogger<TournamentImportQueueTask> _logger;
        private readonly TournamentImportQueueRepository _queueRepository;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRuntimeState _runtimeState;

        public TournamentImportQueueTask(
            ILogger<TournamentImportQueueTask> logger,
            TournamentImportQueueRepository queueRepository,
            ISiteService siteService,
            ISiteAccessor siteAccessor,
            IUmbracoContextFactory umbracoContextFactory,
            IServiceProvider serviceProvider,
            IRuntimeState runtimeState) : base(logger, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30))
        {
            _logger = logger;
            _queueRepository = queueRepository;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _umbracoContextFactory = umbracoContextFactory;
            _serviceProvider = serviceProvider;
            _runtimeState = runtimeState;
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var siteId in _siteService.GetAllSites())
            {
                var pending = _queueRepository.GetPending(siteId);
                if (pending.Count == 0) continue;

                _siteAccessor.SetSiteId(siteId);

                foreach (var row in pending)
                {
                    await ProcessRow(row);
                }
            }
        }

        private async Task ProcessRow(TournamentImportQueueDBModel row)
        {
            // Mark as processing so a second run (or a restart mid-import) doesn't pick it up again.
            _queueRepository.UpdateStatus(row.Id, TournamentImportQueueStatus.Processing, null, null, null);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tournamentService = scope.ServiceProvider.GetRequiredService<TournamentService>();

                var result = await tournamentService.ImportTournament(new ImportTournament
                {
                    FormatId = row.FormatId,
                    Type = row.Type,
                    Source = row.Source,
                    ExternalId = row.ExternalId
                });

                var missingCardsJson = result.MissingCards.Count > 0
                    ? JsonSerializer.Serialize(result.MissingCards)
                    : null;

                _queueRepository.UpdateStatus(
                    row.Id,
                    result.Success ? TournamentImportQueueStatus.Completed : TournamentImportQueueStatus.Failed,
                    result.Message,
                    missingCardsJson,
                    DateTime.UtcNow);

                if (result.Success)
                    _logger.LogInformation("Imported queued tournament {Source}/{ExternalId} (row {RowId})", row.Source, row.ExternalId, row.Id);
                else
                    _logger.LogWarning("Queued tournament import failed for {Source}/{ExternalId} (row {RowId}): {Message}", row.Source, row.ExternalId, row.Id, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error importing queued tournament {Source}/{ExternalId} (row {RowId})", row.Source, row.ExternalId, row.Id);
                _queueRepository.UpdateStatus(row.Id, TournamentImportQueueStatus.Failed, ex.Message, null, DateTime.UtcNow);
            }
        }
    }
}
