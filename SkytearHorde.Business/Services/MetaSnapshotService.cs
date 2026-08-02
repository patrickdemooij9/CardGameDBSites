using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Business.Services
{
    /// <summary>
    /// Builds and persists meta snapshots from tournament data. A snapshot is keyed by
    /// (SiteId, FormatId, PeriodId); recompute fully rebuilds the per-card rows. This is what keeps
    /// usage/winrate cheap to read instead of scanning every tournament on each request.
    /// </summary>
    public class MetaSnapshotService
    {
        private readonly MetaSnapshotRepository _repository;

        public MetaSnapshotService(MetaSnapshotRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Recomputes the snapshot for a period from all of that period's tournament decks. Creates the
        /// snapshot row if none exists yet (this is the "tournament's period differs from the current
        /// snapshot -> new snapshot" behaviour), otherwise updates it in place and replaces its card rows.
        /// </summary>
        public MetaSnapshotResult RecomputeForPeriod(int siteId, int formatId, int periodId)
        {
            var totalDecks = _repository.GetTotalDecks(siteId, periodId);
            var cardStats = _repository.GetCardStats(siteId, periodId).ToArray();

            var now = DateTime.UtcNow;
            var snapshot = _repository.GetSnapshot(siteId, formatId, periodId);
            if (snapshot is null)
            {
                snapshot = new MetaSnapshotDBModel
                {
                    SiteId = siteId,
                    FormatId = formatId,
                    PeriodId = periodId,
                    CreateDateUtc = now
                };
            }

            snapshot.TotalDecks = totalDecks;
            snapshot.LastUpdatedUtc = now;
            _repository.SaveSnapshot(snapshot);

            var rows = cardStats.Select(s => new MetaCardSnapshotDBModel
            {
                SnapshotId = snapshot.Id,
                CardId = s.CardId,
                DeckCount = s.DeckCount,
                Wins = s.Wins,
                Losses = s.Losses
            });
            _repository.ReplaceCardSnapshots(snapshot.Id, rows);

            return new MetaSnapshotResult
            {
                SnapshotId = snapshot.Id,
                PeriodId = periodId,
                TotalDecks = totalDecks,
                CardRowCount = cardStats.Length
            };
        }

        /// <summary>
        /// Reads persisted usage/winrate for a set of cards from the period's snapshot. Every requested
        /// card id gets an entry; ids with no snapshot row (or when no snapshot exists yet) report zeros.
        /// Usage = DeckCount / TotalDecks; winrate = Wins / (Wins + Losses).
        /// </summary>
        public IReadOnlyList<MetaCardStat> GetCardStats(int siteId, int formatId, int periodId, IEnumerable<int> cardIds)
        {
            var ids = cardIds.Distinct().ToArray();
            var result = ids.ToDictionary(id => id, id => new MetaCardStat { CardId = id });

            var snapshot = _repository.GetSnapshot(siteId, formatId, periodId);
            if (snapshot is null || snapshot.TotalDecks == 0)
                return [.. result.Values];

            foreach (var row in _repository.GetCardSnapshots(snapshot.Id, ids))
            {
                var games = row.Wins + row.Losses;
                result[row.CardId] = new MetaCardStat
                {
                    CardId = row.CardId,
                    DeckCount = row.DeckCount,
                    // Unrounded — the frontend rounds and renders sub-1% as "<1%".
                    UsagePercentage = (double)row.DeckCount / snapshot.TotalDecks * 100,
                    WinratePercentage = games == 0 ? 0 : (double)row.Wins / games * 100
                };
            }

            return [.. result.Values];
        }
    }
}
