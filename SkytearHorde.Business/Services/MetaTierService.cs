using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Business.Services
{
    /// <summary>Fetches and assembles; the arithmetic lives in <see cref="MetaTierCalculator"/>.</summary>
    public class MetaTierService
    {
        private readonly MetaSnapshotRepository _snapshotRepository;
        private readonly TournamentRepository _tournamentRepository;
        private readonly PeriodRepository _periodRepository;
        private readonly CardService _cardService;
        private readonly ISiteAccessor _siteAccessor;

        public MetaTierService(
            MetaSnapshotRepository snapshotRepository,
            TournamentRepository tournamentRepository,
            PeriodRepository periodRepository,
            CardService cardService,
            ISiteAccessor siteAccessor)
        {
            _snapshotRepository = snapshotRepository;
            _tournamentRepository = tournamentRepository;
            _periodRepository = periodRepository;
            _cardService = cardService;
            _siteAccessor = siteAccessor;
        }

        public MetaTierList Build(int periodId, int formatId, int leaderGroupId = 1, int leaderSlotId = 0, MetaTierConfig? config = null)
        {
            var cfg = (config ?? new MetaTierConfig()).Clamp();
            var siteId = _siteAccessor.GetSiteId();
            var period = _periodRepository.GetById(periodId);

            var list = new MetaTierList
            {
                PeriodId = periodId,
                PeriodName = period?.Name ?? string.Empty,
                DeltaWeeks = cfg.DeltaWeeks,
                MinDecks = cfg.MinDecks,
                MinEvents = cfg.MinEvents
            };

            var snapshots = _snapshotRepository.GetSnapshots(siteId, formatId, periodId);
            if (snapshots.Count == 0)
            {
                list.DeltasUnavailableReason = "No tournament data has been recorded for this period yet.";
                return list;
            }

            var leaderIds = _tournamentRepository.GetLeaderCardIds(siteId, periodId, leaderGroupId, leaderSlotId).ToHashSet();
            if (leaderIds.Count == 0)
            {
                list.DeltasUnavailableReason = "No decks with an identifiable leader have been recorded yet.";
                return list;
            }

            var latest = snapshots[0];
            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots(snapshots, cfg.DeltaWeeks);

            // One cached read covers every snapshot the delta needs.
            var wanted = new[] { latest, prior, priorPrior }
                .Where(s => s is not null)
                .Select(s => s!.Id)
                .Distinct()
                .ToArray();
            var allRows = _snapshotRepository.GetCardSnapshots(wanted, latest.LastUpdatedUtc);

            var latestRows = RowsFor(allRows, latest.Id, leaderIds);
            var priorRows = prior is null ? null : RowsFor(allRows, prior.Id, leaderIds);
            var priorPriorRows = priorPrior is null ? null : RowsFor(allRows, priorPrior.Id, leaderIds);

            list.DeltasAvailable = priorRows is not null && priorPriorRows is not null;
            list.DeltaComparedToUtc = prior?.SnapshotDateUtc;
            if (!list.DeltasAvailable)
            {
                list.DeltasUnavailableReason = cfg.DeltaWeeks == 1
                    ? "This release is too new to compare week over week. Movement appears once there are two full weeks of results."
                    : $"This release is too new to compare. Movement appears once there are {cfg.DeltaWeeks * 2} full weeks of results.";
            }

            // Denominator is the sum of the numerators, which is the only way shares are guaranteed to total 100%.
            var totalDecks = latestRows.Values.Sum(r => r.DeckCount);
            if (totalDecks == 0)
            {
                list.DeltasUnavailableReason ??= "No decks with an identifiable leader have been recorded yet.";
                return list;
            }

            var recentTotals = list.DeltasAvailable ? MetaTierCalculator.Difference(latestRows, priorRows!) : null;
            var priorTotals = list.DeltasAvailable ? MetaTierCalculator.Difference(priorRows!, priorPriorRows!) : null;
            var recentTotalDecks = recentTotals?.Values.Sum(r => r.DeckCount) ?? 0;
            var priorTotalDecks = priorTotals?.Values.Sum(r => r.DeckCount) ?? 0;

            var leaders = new List<MetaTierLeader>();
            foreach (var (cardId, row) in latestRows)
            {
                var card = _cardService.Get(cardId);
                if (card is null) continue;

                var leader = new MetaTierLeader
                {
                    CardId = cardId,
                    Name = card.DisplayName,
                    DeckCount = row.DeckCount,
                    EventCount = row.EventCount,
                    Wins = row.Wins,
                    Losses = row.Losses,
                    Draws = row.Draws,
                    Top8Count = row.Top8Count,
                    FirstPlaceCount = row.FirstPlaceCount,
                    MetaSharePercentage = (double)row.DeckCount / totalDecks * 100d,
                    WinratePercentage = MetaTierCalculator.Winrate(row.Wins, row.Losses),
                    WinrateLowerBoundPercentage = MetaTierCalculator.WilsonLowerBound(row.Wins, row.Wins + row.Losses) * 100d
                };

                MetaTierCalculator.ApplyDelta(leader, recentTotals, priorTotals, recentTotalDecks, priorTotalDecks, cfg);

                leader.TierScore = MetaTierCalculator.ComputeScore(leader, cfg);
                leader.Tier = MetaTierCalculator.AssignTier(leader, cfg, out var unrankedReason);
                leader.UnrankedReason = unrankedReason;

                leaders.Add(leader);
            }

            // A day ahead because the aggregation's cutoff is exclusive.
            var coverage = _snapshotRepository.GetCoverage(siteId, periodId, DateTime.UtcNow.AddDays(1));
            list.TotalDecks = totalDecks;
            list.TotalEvents = coverage.EventCount;
            list.TotalEntrants = coverage.EntrantCount;
            list.EntrantsWithDeck = coverage.EntrantsWithDeck;
            list.FirstEventUtc = coverage.FirstEventUtc;
            list.LastEventUtc = coverage.LastEventUtc;
            // Newest event, never "now": the title, the on-page stamp and dateModified all read this.
            list.LastUpdatedUtc = coverage.LastEventUtc;

            list.Leaders = [.. leaders
                .OrderBy(l => l.Tier)
                .ThenByDescending(l => l.TierScore)
                .ThenByDescending(l => l.MetaSharePercentage)];

            return list;
        }

        private static Dictionary<int, MetaCardSnapshotDBModel> RowsFor(
            IReadOnlyList<MetaCardSnapshotDBModel> allRows, int snapshotId, HashSet<int> leaderIds)
        {
            return allRows
                .Where(r => r.SnapshotId == snapshotId && leaderIds.Contains(r.CardId))
                .ToDictionary(r => r.CardId);
        }
    }
}
