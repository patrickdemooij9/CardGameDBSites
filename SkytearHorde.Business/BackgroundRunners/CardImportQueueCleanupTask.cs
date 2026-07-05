using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Repositories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    /// <summary>
    /// Removes card import queue items (and their staged images) once they are older than a month.
    /// We don't expect those cards to keep showing up in the monitored channels after that, so there
    /// is no value in keeping the records around for near-duplicate detection any longer.
    /// </summary>
    public class CardImportQueueCleanupTask : RecurringHostedServiceBase
    {
        private static readonly TimeSpan MaxAge = TimeSpan.FromDays(30);

        private readonly CardImportQueueRepository _queueRepository;
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger<CardImportQueueCleanupTask> _logger;

        public CardImportQueueCleanupTask(
            ILogger<CardImportQueueCleanupTask> logger,
            CardImportQueueRepository queueRepository,
            IRuntimeState runtimeState) : base(logger, TimeSpan.FromHours(24), TimeSpan.FromMinutes(5))
        {
            _queueRepository = queueRepository;
            _runtimeState = runtimeState;
            _logger = logger;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            var cutoff = DateTime.UtcNow - MaxAge;
            var oldItems = _queueRepository.GetOlderThan(cutoff);

            foreach (var item in oldItems)
            {
                TryDeleteFile(item.ImagePath);
                TryDeleteFile(item.BackImagePath);
                _queueRepository.Delete(item.Id);
            }

            if (oldItems.Count > 0)
                _logger.LogInformation("Cleaned up {Count} card import queue item(s) older than {Days} days", oldItems.Count, MaxAge.TotalDays);

            return Task.CompletedTask;
        }

        private void TryDeleteFile(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                // Best-effort cleanup; a failed file delete should not stop the row from being removed.
                _logger.LogWarning(ex, "Failed to delete staged card import image {Path}", path);
            }
        }
    }
}
