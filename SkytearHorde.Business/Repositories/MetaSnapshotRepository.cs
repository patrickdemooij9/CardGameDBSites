using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    /// <summary>One snapshot row per (SiteId, FormatId, PeriodId, ISO week), each cumulative from the period start.</summary>
    public class MetaSnapshotRepository
    {
        private const string CachePrefix = "uRepo_MetaSnapshot_";

        private readonly IScopeProvider _scopeProvider;
        private readonly IAppPolicyCache _cache;

        public MetaSnapshotRepository(IScopeProvider scopeProvider, IAppPolicyCache cache)
        {
            _scopeProvider = scopeProvider;
            _cache = cache;
        }

        public MetaSnapshotDBModel? GetLatestSnapshot(int siteId, int formatId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<MetaSnapshotDBModel>(
                "SELECT TOP(1) * FROM MetaSnapshots " +
                "WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2 " +
                "ORDER BY IsLatest DESC, SnapshotDateUtc DESC",
                siteId, formatId, periodId);
        }

        /// <summary>Newest first.</summary>
        public IReadOnlyList<MetaSnapshotDBModel> GetSnapshots(int siteId, int formatId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaSnapshotDBModel>(
                "SELECT * FROM MetaSnapshots " +
                "WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2 " +
                "ORDER BY SnapshotDateUtc DESC",
                siteId, formatId, periodId);
        }

        public MetaSnapshotDBModel? GetSnapshot(int siteId, int formatId, int periodId, DateTime snapshotDateUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<MetaSnapshotDBModel>(
                "SELECT TOP(1) * FROM MetaSnapshots " +
                "WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2 AND SnapshotDateUtc = @3",
                siteId, formatId, periodId, snapshotDateUtc);
        }

        public void SaveSnapshot(MetaSnapshotDBModel snapshot)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            if (snapshot.Id == 0)
            {
                scope.Database.Insert(snapshot);
            }
            else
            {
                scope.Database.Update(snapshot);
            }
        }

        public void SetLatestSnapshot(int siteId, int formatId, int periodId, int snapshotId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "UPDATE MetaSnapshots SET IsLatest = 0 " +
                "WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2 AND Id <> @3",
                siteId, formatId, periodId, snapshotId);
            scope.Database.Execute("UPDATE MetaSnapshots SET IsLatest = 1 WHERE Id = @0", snapshotId);
            scope.Complete();
        }

        public void DeleteSnapshotsForPeriod(int siteId, int formatId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute(
                "DELETE FROM MetaCardSnapshots WHERE SnapshotId IN (" +
                "  SELECT Id FROM MetaSnapshots WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2)",
                siteId, formatId, periodId);
            scope.Database.Execute(
                "DELETE FROM MetaSnapshots WHERE SiteId = @0 AND FormatId = @1 AND PeriodId = @2",
                siteId, formatId, periodId);
            scope.Complete();
        }

        public IEnumerable<MetaCardSnapshotDBModel> GetCardSnapshots(int snapshotId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaCardSnapshotDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<MetaCardSnapshotDBModel>()
                .Where<MetaCardSnapshotDBModel>(x => x.SnapshotId == snapshotId));
        }

        public IEnumerable<MetaCardSnapshotDBModel> GetCardSnapshots(int snapshotId, int[] cardIds)
        {
            if (cardIds.Length == 0) return [];

            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            // SQL Server caps a request at 2100 parameters and each id is one.
            var results = new List<MetaCardSnapshotDBModel>();
            foreach (var chunk in cardIds.Chunk(2000))
            {
                results.AddRange(scope.Database.Fetch<MetaCardSnapshotDBModel>(
                    "SELECT * FROM MetaCardSnapshots WHERE SnapshotId = @0 AND CardId IN (@1)",
                    snapshotId, chunk));
            }
            return results;
        }

        /// <summary>Pass the latest snapshot's LastUpdatedUtc as <paramref name="cacheStamp"/> so a recompute invalidates the cache.</summary>
        public IReadOnlyList<MetaCardSnapshotDBModel> GetCardSnapshots(int[] snapshotIds, DateTime cacheStamp)
        {
            if (snapshotIds.Length == 0) return [];

            var cacheKey = $"{CachePrefix}Cards_{string.Join('-', snapshotIds.OrderBy(x => x))}_{cacheStamp.Ticks}";
            var cached = _cache.GetCacheItem<List<MetaCardSnapshotDBModel>>(cacheKey);
            if (cached != null) return cached;

            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var result = scope.Database.Fetch<MetaCardSnapshotDBModel>(
                "SELECT * FROM MetaCardSnapshots WHERE SnapshotId IN (@0)", snapshotIds);

            _cache.Insert(cacheKey, () => result, TimeSpan.FromHours(1));
            return result;
        }

        public void ReplaceCardSnapshots(int snapshotId, IEnumerable<MetaCardSnapshotDBModel> rows)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM MetaCardSnapshots WHERE SnapshotId = @0", snapshotId);
            foreach (var row in rows)
            {
                row.SnapshotId = snapshotId;
                scope.Database.Insert(row);
            }
            scope.Complete();
        }

        public void ClearCache() => _cache.ClearByKey(CachePrefix);

        // asOfUtc is an exclusive upper bound, so the same query can rebuild any past week.
        public int GetTotalDecks(int siteId, int periodId, DateTime asOfUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(DISTINCT dv.VersionId) " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "CROSS APPLY (SELECT TOP(1) dv2.Id AS VersionId FROM DeckVersion dv2 " +
                "             WHERE dv2.DeckId = d.Id AND dv2.IsCurrent = 1 ORDER BY dv2.Id DESC) dv " +
                "WHERE t.SiteId = @0 AND t.PeriodId = @1 AND t.DateUtc < @2",
                siteId, periodId, asOfUtc);
        }

        // DISTINCT and CROSS APPLY TOP(1) both guard double-counting: a plain join on IsCurrent = 1
        // doubles an entrant whose deck has two current versions.
        public IEnumerable<MetaCardStatRow> GetCardStats(int siteId, int periodId, DateTime asOfUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaCardStatRow>(
                "SELECT s.CardId AS CardId, " +
                "  COUNT(DISTINCT s.VersionId) AS DeckCount, " +
                "  COUNT(DISTINCT s.TournamentId) AS EventCount, " +
                "  SUM(s.Wins) AS Wins, " +
                "  SUM(s.Losses) AS Losses, " +
                "  SUM(s.Draws) AS Draws, " +
                "  SUM(CASE WHEN s.Placement BETWEEN 1 AND 8 THEN 1 ELSE 0 END) AS Top8Count, " +
                "  SUM(CASE WHEN s.Placement = 1 THEN 1 ELSE 0 END) AS FirstPlaceCount " +
                "FROM ( " +
                "  SELECT DISTINCT te.Id AS EntrantId, t.Id AS TournamentId, dv.VersionId AS VersionId, " +
                "         dc.CardId AS CardId, te.Wins AS Wins, te.Losses AS Losses, te.Draws AS Draws, " +
                "         te.Placement AS Placement " +
                "  FROM TournamentEntrants te " +
                "  INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "  INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "  CROSS APPLY (SELECT TOP(1) dv2.Id AS VersionId FROM DeckVersion dv2 " +
                "               WHERE dv2.DeckId = d.Id AND dv2.IsCurrent = 1 ORDER BY dv2.Id DESC) dv " +
                "  INNER JOIN DeckCard dc ON dc.VersionId = dv.VersionId AND dc.GroupId < 99 " +
                "  WHERE t.SiteId = @0 AND t.PeriodId = @1 AND t.DateUtc < @2 " +
                ") s " +
                "GROUP BY s.CardId",
                siteId, periodId, asOfUtc);
        }

        public MetaCoverageRow GetCoverage(int siteId, int periodId, DateTime asOfUtc)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<MetaCoverageRow>(
                "SELECT COUNT(DISTINCT t.Id) AS EventCount, " +
                "  COUNT(*) AS EntrantCount, " +
                "  SUM(CASE WHEN te.TournamentDeckId IS NULL THEN 0 ELSE 1 END) AS EntrantsWithDeck, " +
                "  MIN(t.DateUtc) AS FirstEventUtc, " +
                "  MAX(t.DateUtc) AS LastEventUtc " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "WHERE t.SiteId = @0 AND t.PeriodId = @1 AND t.DateUtc < @2",
                siteId, periodId, asOfUtc) ?? new MetaCoverageRow();
        }

        public DateTime? GetLatestTournamentDate(int siteId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.ExecuteScalar<DateTime?>(
                "SELECT MAX(DateUtc) FROM Tournaments WHERE SiteId = @0 AND PeriodId = @1",
                siteId, periodId);
        }
    }
}
