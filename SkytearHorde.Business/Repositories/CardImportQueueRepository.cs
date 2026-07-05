using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SkytearHorde.Business.Repositories
{
    public class CardImportQueueRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public CardImportQueueRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public int Insert(CardImportQueueDBModel item)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(item);
            scope.Complete();
            return item.Id;
        }

        public List<CardImportQueueDBModel> GetPending(int siteId)
        {
            using var scope = _scopeProvider.CreateScope();
            var results = scope.Database.Fetch<CardImportQueueDBModel>(
                "WHERE SiteId = @0 AND Status IN (@1, @2) ORDER BY CreatedAt ASC",
                siteId, CardImportQueueStatus.Pending, CardImportQueueStatus.PotentialVariant);
            scope.Complete();
            return results;
        }

        public CardImportQueueDBModel? GetById(int id)
        {
            using var scope = _scopeProvider.CreateScope();
            var result = scope.Database.FirstOrDefault<CardImportQueueDBModel>("WHERE Id = @0", id);
            scope.Complete();
            return result;
        }

        public void UpdateStatus(int id, string status)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE CardImportQueue SET Status = @0, ReviewedAt = @1 WHERE Id = @2",
                status, DateTime.UtcNow, id);
            scope.Complete();
        }

        public void UpdateExtractedData(int id, string extractedDataJson)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE CardImportQueue SET ExtractedData = @0 WHERE Id = @1",
                extractedDataJson, id);
            scope.Complete();
        }

        public void UpdateMatch(int id, string status, int? potentialDuplicateId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE CardImportQueue SET Status = @0, PotentialDuplicateId = @1 WHERE Id = @2",
                status, potentialDuplicateId, id);
            scope.Complete();
        }

        public void UpdateSet(int id, int setId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE CardImportQueue SET SetId = @0 WHERE Id = @1",
                setId, id);
            scope.Complete();
        }

        public void Delete(int id)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM CardImportQueue WHERE Id = @0", id);
            scope.Complete();
        }

        /// <summary>Returns all queue items created before the given cutoff, regardless of status.</summary>
        public List<CardImportQueueDBModel> GetOlderThan(DateTime cutoff)
        {
            using var scope = _scopeProvider.CreateScope();
            var results = scope.Database.Fetch<CardImportQueueDBModel>(
                "WHERE CreatedAt < @0", cutoff);
            scope.Complete();
            return results;
        }

        /// <summary>
        /// Returns any existing queue items with a near-identical image hash (Hamming distance ≤ threshold).
        /// Used to detect the same card image arriving from multiple sources.
        /// </summary>
        public bool HashExists(int siteId, string imageHash, int hammingThreshold = 10)
        {
            using var scope = _scopeProvider.CreateScope();
            var candidates = scope.Database.Fetch<CardImportQueueDBModel>(
                "WHERE SiteId = @0 AND ImageHash IS NOT NULL AND Status != @1",
                siteId, CardImportQueueStatus.Rejected);
            scope.Complete();

            return candidates.Any(c =>
                c.ImageHash != null &&
                HammingDistance(c.ImageHash, imageHash) <= hammingThreshold);
        }

        private static int HammingDistance(string a, string b)
        {
            if (a.Length != b.Length) return int.MaxValue;
            return a.Zip(b).Count(pair => pair.First != pair.Second);
        }
    }
}
