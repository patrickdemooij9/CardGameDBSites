using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Business.Services
{
    public class MetaSnapshotService
    {
        private readonly MetaSnapshotRepository _repository;
        private readonly PeriodRepository _periodRepository;

        public MetaSnapshotService(MetaSnapshotRepository repository, PeriodRepository periodRepository)
        {
            _repository = repository;
            _periodRepository = periodRepository;
        }

        /// <summary>Rewrites the current week's row in place; a week roll-over inserts a new one, freezing the previous week as the delta baseline.</summary>
        public MetaSnapshotResult RecomputeForPeriod(int siteId, int formatId, int periodId)
        {
            var weekStart = MetaWeek.StartOfWeek(DateTime.UtcNow);
            var result = BuildSnapshot(siteId, formatId, periodId, weekStart);
            _repository.SetLatestSnapshot(siteId, formatId, periodId, result.SnapshotId);
            _repository.ClearCache();
            return result;
        }

        /// <summary>Drops and rebuilds a period's whole weekly history. Idempotent, and the repair path for a tournament that imported into the wrong week.</summary>
        public MetaSnapshotResult[] BackfillForPeriod(int siteId, int formatId, int periodId)
        {
            var period = _periodRepository.GetById(periodId);
            if (period is null) return [];

            // Anchor on the newest tournament, not UtcNow: melee imports lag by days.
            var lastEvent = _repository.GetLatestTournamentDate(siteId, periodId);
            if (lastEvent is null) return [];

            var end = period.EndDateUtc is null || period.EndDateUtc > lastEvent
                ? lastEvent.Value
                : period.EndDateUtc.Value;

            _repository.DeleteSnapshotsForPeriod(siteId, formatId, periodId);

            var results = new List<MetaSnapshotResult>();
            foreach (var weekStart in MetaWeek.WeeksBetween(period.StartingDateUtc, end))
            {
                results.Add(BuildSnapshot(siteId, formatId, periodId, weekStart));
            }

            if (results.Count > 0)
            {
                _repository.SetLatestSnapshot(siteId, formatId, periodId, results[^1].SnapshotId);
            }
            _repository.ClearCache();

            return [.. results];
        }

        private MetaSnapshotResult BuildSnapshot(int siteId, int formatId, int periodId, DateTime weekStartUtc)
        {
            var asOf = MetaWeek.EndOfWeek(weekStartUtc);
            var totalDecks = _repository.GetTotalDecks(siteId, periodId, asOf);
            var cardStats = _repository.GetCardStats(siteId, periodId, asOf).ToArray();

            var now = DateTime.UtcNow;
            var snapshot = _repository.GetSnapshot(siteId, formatId, periodId, weekStartUtc);
            if (snapshot is null)
            {
                snapshot = new MetaSnapshotDBModel
                {
                    SiteId = siteId,
                    FormatId = formatId,
                    PeriodId = periodId,
                    SnapshotDateUtc = weekStartUtc,
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
                EventCount = s.EventCount,
                Wins = s.Wins,
                Losses = s.Losses,
                Draws = s.Draws,
                Top8Count = s.Top8Count,
                FirstPlaceCount = s.FirstPlaceCount
            });
            _repository.ReplaceCardSnapshots(snapshot.Id, rows);

            return new MetaSnapshotResult
            {
                SnapshotId = snapshot.Id,
                PeriodId = periodId,
                SnapshotDateUtc = weekStartUtc,
                TotalDecks = totalDecks,
                CardRowCount = cardStats.Length
            };
        }

        /// <summary>Every requested id gets an entry; ids with no snapshot row report zeros.</summary>
        public IReadOnlyList<MetaCardStat> GetCardStats(int siteId, int formatId, int periodId, IEnumerable<int> cardIds)
        {
            var ids = cardIds.Distinct().ToArray();
            var result = ids.ToDictionary(id => id, id => new MetaCardStat { CardId = id });

            var snapshot = _repository.GetLatestSnapshot(siteId, formatId, periodId);
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
