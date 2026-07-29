using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SkytearHorde.Business.Repositories
{
    public class TournamentImportQueueRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TournamentImportQueueRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public int Insert(TournamentImportQueueDBModel item)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(item);
            scope.Complete();
            return item.Id;
        }

        public List<TournamentImportQueueDBModel> GetPending(int siteId)
        {
            using var scope = _scopeProvider.CreateScope();
            var results = scope.Database.Fetch<TournamentImportQueueDBModel>(
                "WHERE SiteId = @0 AND Status = @1 ORDER BY CreatedAt ASC",
                siteId, TournamentImportQueueStatus.Pending);
            scope.Complete();
            return results;
        }

        public void UpdateStatus(int id, string status, string? message, string? missingCardsJson, DateTime? processedAt)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE TournamentImportQueue SET Status = @0, Message = @1, MissingCards = @2, ProcessedAt = @3 WHERE Id = @4",
                status, message, missingCardsJson, processedAt, id);
            scope.Complete();
        }

        /// <summary>True when a row for this source + external id is still waiting or being processed.</summary>
        public bool ExistsPending(int siteId, string source, string externalId)
        {
            using var scope = _scopeProvider.CreateScope();
            var count = scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM TournamentImportQueue " +
                "WHERE SiteId = @0 AND Source = @1 AND ExternalId = @2 AND Status IN (@3, @4)",
                siteId, source, externalId,
                TournamentImportQueueStatus.Pending, TournamentImportQueueStatus.Processing);
            scope.Complete();
            return count > 0;
        }
    }
}
