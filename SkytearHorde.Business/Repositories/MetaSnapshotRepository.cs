using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    /// <summary>
    /// Persists and reads meta snapshots, and runs the tournament aggregations used to (re)build them.
    /// Follows the hand-written NPoco pattern of <see cref="TournamentRepository"/>.
    /// </summary>
    public class MetaSnapshotRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public MetaSnapshotRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        // --- Snapshot rows ---

        public MetaSnapshotDBModel? GetSnapshot(int siteId, int formatId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<MetaSnapshotDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<MetaSnapshotDBModel>()
                .Where<MetaSnapshotDBModel>(x => x.SiteId == siteId && x.FormatId == formatId && x.PeriodId == periodId));
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

        // --- Card rows ---

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
            // Each id in the IN clause becomes its own SQL parameter; SQL Server caps a request at 2100.
            // Chunk the ids (leaving room for the snapshotId parameter) so large lists don't blow the limit.
            var results = new List<MetaCardSnapshotDBModel>();
            foreach (var chunk in cardIds.Chunk(2000))
            {
                results.AddRange(scope.Database.Fetch<MetaCardSnapshotDBModel>(
                    "SELECT * FROM MetaCardSnapshots WHERE SnapshotId = @0 AND CardId IN (@1)",
                    snapshotId, chunk));
            }
            return results;
        }

        /// <summary>
        /// Replaces all card rows for a snapshot: recompute is a full rebuild, so the old rows are
        /// deleted and the new set inserted inside one scope.
        /// </summary>
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

        // --- Aggregations over the period's tournament decks ---
        // Join path mirrors TournamentRepository: TournamentEntrants -> Deck -> DeckVersion (IsCurrent=1) -> DeckCard.

        public int GetTotalDecks(int siteId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.ExecuteScalar<int>(
                "SELECT COUNT(DISTINCT dv.Id) " +
                "FROM TournamentEntrants te " +
                "INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "INNER JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "WHERE t.SiteId = @0 AND t.PeriodId = @1",
                siteId, periodId);
        }

        /// <summary>
        /// Per-card usage and win/loss record over every main-deck card in the period.
        /// The inner query is DISTINCT on (entrant, version, card) so an entrant's record is counted
        /// once per card even if the same card sits in two slots.
        /// </summary>
        public IEnumerable<MetaCardStatRow> GetCardStats(int siteId, int periodId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<MetaCardStatRow>(
                "SELECT s.CardId AS CardId, " +
                "  COUNT(DISTINCT s.VersionId) AS DeckCount, " +
                "  SUM(s.Wins) AS Wins, " +
                "  SUM(s.Losses) AS Losses " +
                "FROM ( " +
                "  SELECT DISTINCT te.Id AS EntrantId, dv.Id AS VersionId, dc.CardId AS CardId, te.Wins AS Wins, te.Losses AS Losses " +
                "  FROM TournamentEntrants te " +
                "  INNER JOIN Tournaments t ON t.Id = te.TournamentId " +
                "  INNER JOIN Deck d ON d.Id = te.TournamentDeckId " +
                "  INNER JOIN DeckVersion dv ON dv.DeckId = d.Id AND dv.IsCurrent = 1 " +
                "  INNER JOIN DeckCard dc ON dc.VersionId = dv.Id AND dc.GroupId < 99 " +
                "  WHERE t.SiteId = @0 AND t.PeriodId = @1 " +
                ") s " +
                "GROUP BY s.CardId",
                siteId, periodId);
        }
    }

    // Raw fetch row for the per-card aggregation.
    public class MetaCardStatRow
    {
        public int CardId { get; set; }
        public int DeckCount { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}
