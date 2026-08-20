using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public static class MetaTierCalculator
    {
        // A Bo3 is not three independent trials, so shrink n before bounding or the interval comes out too tight.
        public const double GameDesignEffect = 1.7d;

        /// <summary>z for a one-sided 90% bound.</summary>
        public const double WilsonZ = 1.2815515655446004d;

        /// <summary>Returns a proportion in [0, 1]; 0 when there are no games.</summary>
        public static double WilsonLowerBound(int wins, int games)
        {
            if (games <= 0) return 0d;

            var n = games / GameDesignEffect;
            var p = Math.Clamp((double)wins / games, 0d, 1d);

            var z2 = WilsonZ * WilsonZ;
            var centre = p + z2 / (2d * n);
            var margin = WilsonZ * Math.Sqrt((p * (1d - p) + z2 / (4d * n)) / n);

            return Math.Clamp((centre - margin) / (1d + z2 / n), 0d, 1d);
        }

        /// <summary>Draws are excluded. Must stay in sync with MetaSnapshotService.GetCardStats.</summary>
        public static double Winrate(int wins, int losses)
        {
            var games = wins + losses;
            return games == 0 ? 0d : (double)wins / games * 100d;
        }

        public static double ComputeScore(MetaTierLeader leader, MetaTierConfig cfg)
        {
            // Raw win rate, not the bound: over the narrow band real win rates occupy the bound tracks
            // sample size more than strength, so ranking by it would rank by popularity.
            var performance = leader.WinratePercentage - 50d;

            // Log-damped so a dominant archetype's share doesn't swamp the scale.
            var presence = Math.Log10(1d + leader.MetaSharePercentage) * 10d;

            var conversion = leader.DeckCount == 0 ? 0d : (double)leader.Top8Count / leader.DeckCount * 100d;

            return performance
                 + presence * cfg.PresenceWeight * 0.5d
                 + conversion * cfg.ConversionWeight;
        }

        public static MetaTier AssignTier(MetaTierLeader leader, MetaTierConfig cfg, out string? unrankedReason)
        {
            unrankedReason = null;

            // The gate, not the confidence bound, is what keeps a thin leader off the chart.
            if (leader.DeckCount < cfg.MinDecks || leader.EventCount < cfg.MinEvents)
            {
                unrankedReason = leader.DeckCount < cfg.MinDecks
                    ? $"Only {leader.DeckCount} deck{(leader.DeckCount == 1 ? "" : "s")} recorded (needs {cfg.MinDecks})."
                    : $"Only seen at {leader.EventCount} event{(leader.EventCount == 1 ? "" : "s")} (needs {cfg.MinEvents}).";
                return MetaTier.Unranked;
            }

            var tier = leader.TierScore >= cfg.STierScore ? MetaTier.S
                     : leader.TierScore >= cfg.ATierScore ? MetaTier.A
                     : leader.TierScore >= cfg.BTierScore ? MetaTier.B
                     : leader.TierScore >= cfg.CTierScore ? MetaTier.C
                     : MetaTier.D;

            // Clears the gate but doesn't survive the bound: riding variance, so strong but not top tier.
            if (tier == MetaTier.S && leader.WinrateLowerBoundPercentage < 50d)
            {
                tier = MetaTier.A;
            }

            return tier;
        }

        /// <summary>Turns two cumulative snapshots into the disjoint sample for the window between them.</summary>
        public static Dictionary<int, MetaCardSnapshotDBModel> Difference(
            IReadOnlyDictionary<int, MetaCardSnapshotDBModel> newer,
            IReadOnlyDictionary<int, MetaCardSnapshotDBModel> older)
        {
            var result = new Dictionary<int, MetaCardSnapshotDBModel>();
            foreach (var (cardId, row) in newer)
            {
                older.TryGetValue(cardId, out var before);
                result[cardId] = new MetaCardSnapshotDBModel
                {
                    CardId = cardId,
                    // Clamped: a re-imported tournament can make a cumulative count tick down.
                    DeckCount = Math.Max(0, row.DeckCount - (before?.DeckCount ?? 0)),
                    EventCount = Math.Max(0, row.EventCount - (before?.EventCount ?? 0)),
                    Wins = Math.Max(0, row.Wins - (before?.Wins ?? 0)),
                    Losses = Math.Max(0, row.Losses - (before?.Losses ?? 0)),
                    Draws = Math.Max(0, row.Draws - (before?.Draws ?? 0)),
                    Top8Count = Math.Max(0, row.Top8Count - (before?.Top8Count ?? 0)),
                    FirstPlaceCount = Math.Max(0, row.FirstPlaceCount - (before?.FirstPlaceCount ?? 0))
                };
            }
            return result;
        }

        /// <summary>Selects by date, not position: a week with no imports leaves no row, so counting back N entries would compare the wrong spans.</summary>
        public static (MetaSnapshotDBModel? Prior, MetaSnapshotDBModel? PriorPrior) SelectDeltaSnapshots(
            IReadOnlyList<MetaSnapshotDBModel> newestFirst, int deltaWeeks)
        {
            if (newestFirst.Count == 0) return (null, null);

            var days = deltaWeeks * 7;
            var latest = newestFirst[0];

            var prior = newestFirst.FirstOrDefault(s => s.SnapshotDateUtc <= latest.SnapshotDateUtc.AddDays(-days));
            if (prior is null) return (null, null);

            var priorPrior = newestFirst.FirstOrDefault(s => s.SnapshotDateUtc <= prior.SnapshotDateUtc.AddDays(-days));
            return (prior, priorPrior);
        }

        /// <summary>Deltas are in percentage points, and null when either window is too thin to justify an arrow.</summary>
        public static void ApplyDelta(
            MetaTierLeader leader,
            IReadOnlyDictionary<int, MetaCardSnapshotDBModel>? recent,
            IReadOnlyDictionary<int, MetaCardSnapshotDBModel>? prior,
            int recentTotalDecks,
            int priorTotalDecks,
            MetaTierConfig cfg)
        {
            if (recent is null || prior is null || recentTotalDecks == 0 || priorTotalDecks == 0) return;

            recent.TryGetValue(leader.CardId, out var recentRow);
            prior.TryGetValue(leader.CardId, out var priorRow);

            var recentDecks = recentRow?.DeckCount ?? 0;
            var priorDecks = priorRow?.DeckCount ?? 0;

            leader.IsNewEntry = recentDecks > 0 && priorDecks == 0;

            // Weekly increments are only a couple of events wide; without this they produce confident-looking noise.
            if (recentDecks < cfg.MinDecksForDelta || priorDecks < cfg.MinDecksForDelta) return;

            leader.MetaShareDeltaPoints =
                ((double)recentDecks / recentTotalDecks * 100d) - ((double)priorDecks / priorTotalDecks * 100d);

            var recentGames = (recentRow?.Wins ?? 0) + (recentRow?.Losses ?? 0);
            var priorGames = (priorRow?.Wins ?? 0) + (priorRow?.Losses ?? 0);
            if (recentGames > 0 && priorGames > 0)
            {
                leader.WinrateDeltaPoints =
                    Winrate(recentRow!.Wins, recentRow.Losses) - Winrate(priorRow!.Wins, priorRow.Losses);
            }
        }
    }
}
