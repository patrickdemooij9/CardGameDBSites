using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class MetaTierCalculatorTests
    {
        private static MetaTierConfig Config() => new MetaTierConfig().Clamp();

        private static MetaTierLeader Leader(
            int cardId = 1, int deckCount = 100, int eventCount = 10,
            double winrate = 50d, double lowerBound = 50d, double metaShare = 10d,
            int top8 = 0, double score = 0d)
        {
            return new MetaTierLeader
            {
                CardId = cardId,
                DeckCount = deckCount,
                EventCount = eventCount,
                WinratePercentage = winrate,
                WinrateLowerBoundPercentage = lowerBound,
                MetaSharePercentage = metaShare,
                Top8Count = top8,
                TierScore = score
            };
        }

        private static MetaCardSnapshotDBModel Row(
            int cardId, int deckCount, int wins = 0, int losses = 0,
            int eventCount = 0, int top8 = 0, int first = 0, int draws = 0)
        {
            return new MetaCardSnapshotDBModel
            {
                CardId = cardId,
                DeckCount = deckCount,
                Wins = wins,
                Losses = losses,
                Draws = draws,
                EventCount = eventCount,
                Top8Count = top8,
                FirstPlaceCount = first
            };
        }

        private static MetaSnapshotDBModel Snapshot(int id, string weekStart) => new()
        {
            Id = id,
            SnapshotDateUtc = DateTime.Parse(weekStart)
        };

        // ── Winrate ──────────────────────────────────────────────────────────

        [Test]
        public void Winrate_NoGames_ReturnsZero()
        {
            Assert.That(MetaTierCalculator.Winrate(0, 0), Is.EqualTo(0d));
        }

        [Test]
        public void Winrate_ExcludesDraws_ByOnlyTakingWinsAndLosses()
        {
            // Draws are deliberately not a parameter: the winrate must match MetaSnapshotService, which
            // uses Wins/(Wins+Losses), or /meta/ and /meta/leaders/{x} would disagree for one leader.
            Assert.That(MetaTierCalculator.Winrate(30, 20), Is.EqualTo(60d));
        }

        // ── Wilson lower bound ───────────────────────────────────────────────

        [Test]
        public void WilsonLowerBound_NoGames_ReturnsZero()
        {
            Assert.That(MetaTierCalculator.WilsonLowerBound(0, 0), Is.EqualTo(0d));
        }

        [Test]
        public void WilsonLowerBound_SingleWin_IsFarBelowOne()
        {
            // 1/1 is a 100% win rate on no evidence; the bound must not endorse it.
            var bound = MetaTierCalculator.WilsonLowerBound(1, 1);

            Assert.That(bound, Is.LessThan(0.5d));
            Assert.That(bound, Is.GreaterThan(0d));
        }

        [Test]
        public void WilsonLowerBound_AlwaysBelowObservedRate()
        {
            foreach (var games in new[] { 1, 10, 60, 200, 500 })
            {
                var wins = (int)Math.Round(games * 0.55);
                var bound = MetaTierCalculator.WilsonLowerBound(wins, games) * 100d;

                Assert.That(bound, Is.LessThan(55d), $"games={games}");
            }
        }

        [Test]
        public void WilsonLowerBound_TightensAsSampleGrows()
        {
            var small = MetaTierCalculator.WilsonLowerBound(33, 60);     // 55% on 60 games
            var large = MetaTierCalculator.WilsonLowerBound(275, 500);   // 55% on 500 games

            Assert.That(large, Is.GreaterThan(small));
        }

        [Test]
        public void WilsonLowerBound_SmallSampleAtFiftyFivePercent_FallsBelowFifty()
        {
            // This is the case the S-tier guard exists for: a 55% win rate on a thin sample.
            var bound = MetaTierCalculator.WilsonLowerBound(33, 60) * 100d;

            Assert.That(bound, Is.LessThan(50d));
        }

        [Test]
        public void WilsonLowerBound_LargeSampleAtFiftyFivePercent_ClearsFifty()
        {
            var bound = MetaTierCalculator.WilsonLowerBound(275, 500) * 100d;

            Assert.That(bound, Is.GreaterThan(50d));
        }

        [Test]
        public void WilsonLowerBound_StaysWithinUnitInterval()
        {
            foreach (var (wins, games) in new[] { (0, 10), (10, 10), (1, 500), (500, 500) })
            {
                var bound = MetaTierCalculator.WilsonLowerBound(wins, games);

                Assert.That(bound, Is.GreaterThanOrEqualTo(0d));
                Assert.That(bound, Is.LessThanOrEqualTo(1d));
            }
        }

        // ── Sample gate ──────────────────────────────────────────────────────

        [Test]
        public void AssignTier_BelowMinDecks_IsUnrankedAndSaysWhy()
        {
            var cfg = Config();
            var leader = Leader(deckCount: cfg.MinDecks - 1, score: 99d);

            var tier = MetaTierCalculator.AssignTier(leader, cfg, out var reason);

            Assert.That(tier, Is.EqualTo(MetaTier.Unranked));
            Assert.That(reason, Does.Contain("deck"));
        }

        [Test]
        public void AssignTier_BelowMinEvents_IsUnranked()
        {
            var cfg = Config();
            var leader = Leader(deckCount: 500, eventCount: cfg.MinEvents - 1, score: 99d);

            var tier = MetaTierCalculator.AssignTier(leader, cfg, out var reason);

            Assert.That(tier, Is.EqualTo(MetaTier.Unranked));
            Assert.That(reason, Does.Contain("event"));
        }

        [Test]
        public void AssignTier_HugeScoreOnTinySample_StillUnranked()
        {
            // The gate, not the confidence bound, is what keeps a 4-deck leader off the chart.
            var cfg = Config();
            var leader = Leader(deckCount: 4, eventCount: 1, winrate: 100d, lowerBound: 100d, score: 500d);

            Assert.That(MetaTierCalculator.AssignTier(leader, cfg, out _), Is.EqualTo(MetaTier.Unranked));
        }

        [Test]
        public void AssignTier_SingularReasonForOneDeck()
        {
            var leader = Leader(deckCount: 1);

            MetaTierCalculator.AssignTier(leader, Config(), out var reason);

            Assert.That(reason, Does.Contain("1 deck "));
            Assert.That(reason, Does.Not.Contain("1 decks"));
        }

        // ── Tier boundaries ──────────────────────────────────────────────────

        [Test]
        public void AssignTier_ScoreExactlyOnThreshold_TakesHigherTier()
        {
            var cfg = Config();
            var leader = Leader(score: cfg.ATierScore, lowerBound: 60d);

            Assert.That(MetaTierCalculator.AssignTier(leader, cfg, out _), Is.EqualTo(MetaTier.A));
        }

        [TestCase(6.0, MetaTier.S)]
        [TestCase(3.0, MetaTier.A)]
        [TestCase(1.0, MetaTier.B)]
        [TestCase(-1.0, MetaTier.C)]
        [TestCase(-10.0, MetaTier.D)]
        public void AssignTier_MapsScoreToTier(double score, MetaTier expected)
        {
            var leader = Leader(score: score, lowerBound: 60d);

            Assert.That(MetaTierCalculator.AssignTier(leader, Config(), out _), Is.EqualTo(expected));
        }

        // ── S-tier demotion guard ────────────────────────────────────────────

        [Test]
        public void AssignTier_STierScoreButWeakLowerBound_DemotedToA()
        {
            var leader = Leader(score: 99d, lowerBound: 49.9d);

            Assert.That(MetaTierCalculator.AssignTier(leader, Config(), out _), Is.EqualTo(MetaTier.A));
        }

        [Test]
        public void AssignTier_STierScoreWithStrongLowerBound_StaysS()
        {
            var leader = Leader(score: 99d, lowerBound: 50.1d);

            Assert.That(MetaTierCalculator.AssignTier(leader, Config(), out _), Is.EqualTo(MetaTier.S));
        }

        [Test]
        public void AssignTier_GuardOnlyAppliesToSTier()
        {
            // A weak bound must not cascade demotions through the lower tiers.
            var leader = Leader(score: 3.0d, lowerBound: 20d);

            Assert.That(MetaTierCalculator.AssignTier(leader, Config(), out _), Is.EqualTo(MetaTier.A));
        }

        // ── Score ────────────────────────────────────────────────────────────

        [Test]
        public void ComputeScore_HigherWinrateScoresHigher_AtEqualShare()
        {
            var cfg = Config();
            var weak = Leader(winrate: 48d, metaShare: 10d);
            var strong = Leader(winrate: 56d, metaShare: 10d);

            Assert.That(MetaTierCalculator.ComputeScore(strong, cfg),
                Is.GreaterThan(MetaTierCalculator.ComputeScore(weak, cfg)));
        }

        [Test]
        public void ComputeScore_PresenceIsLogDamped_SoShareCannotSwampWinrate()
        {
            var cfg = Config();
            // A 30% share is 6x a 5% share, but must not be worth 6x the score contribution.
            var niche = Leader(winrate: 50d, metaShare: 5d);
            var dominant = Leader(winrate: 50d, metaShare: 30d);

            var nicheScore = MetaTierCalculator.ComputeScore(niche, cfg);
            var dominantScore = MetaTierCalculator.ComputeScore(dominant, cfg);

            Assert.That(dominantScore, Is.GreaterThan(nicheScore));
            Assert.That(dominantScore, Is.LessThan(nicheScore * 3));
        }

        [Test]
        public void ComputeScore_ConversionIgnoredByDefault()
        {
            // Top-8 conversion is meaningless at a 12-player local, so its weight defaults to 0.
            var cfg = Config();
            var noTop8 = Leader(top8: 0);
            var allTop8 = Leader(top8: 100);

            Assert.That(MetaTierCalculator.ComputeScore(allTop8, cfg),
                Is.EqualTo(MetaTierCalculator.ComputeScore(noTop8, cfg)));
        }

        // ── Config clamping ──────────────────────────────────────────────────

        [Test]
        public void Clamp_ZeroMinDecks_IsRaisedToASafeFloor()
        {
            var cfg = new MetaTierConfig { MinDecks = 0 }.Clamp();

            Assert.That(cfg.MinDecks, Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public void Clamp_NonMonotonicThresholds_FallBackToDefaults()
        {
            var cfg = new MetaTierConfig { STierScore = 0d, ATierScore = 10d }.Clamp();

            Assert.That(cfg.STierScore, Is.EqualTo(5.0d));
            Assert.That(cfg.ATierScore, Is.EqualTo(2.5d));
        }

        [Test]
        public void Clamp_ValidThresholds_ArePreserved()
        {
            var cfg = new MetaTierConfig { STierScore = 8d, ATierScore = 4d, BTierScore = 1d, CTierScore = -2d }.Clamp();

            Assert.That(cfg.STierScore, Is.EqualTo(8d));
            Assert.That(cfg.CTierScore, Is.EqualTo(-2d));
        }

        // ── Snapshot subtraction ─────────────────────────────────────────────

        [Test]
        public void Difference_SubtractsCumulativeCounts()
        {
            var newer = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 100, wins: 300, losses: 200, eventCount: 12) };
            var older = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 80, wins: 240, losses: 160, eventCount: 9) };

            var diff = MetaTierCalculator.Difference(newer, older);

            Assert.That(diff[1].DeckCount, Is.EqualTo(20));
            Assert.That(diff[1].Wins, Is.EqualTo(60));
            Assert.That(diff[1].Losses, Is.EqualTo(40));
            Assert.That(diff[1].EventCount, Is.EqualTo(3));
        }

        [Test]
        public void Difference_LeaderAbsentFromOlderSnapshot_CarriesFullTotals()
        {
            var newer = new Dictionary<int, MetaCardSnapshotDBModel> { [7] = Row(7, 15, wins: 40, losses: 30) };
            var older = new Dictionary<int, MetaCardSnapshotDBModel>();

            var diff = MetaTierCalculator.Difference(newer, older);

            Assert.That(diff[7].DeckCount, Is.EqualTo(15));
            Assert.That(diff[7].Wins, Is.EqualTo(40));
        }

        [Test]
        public void Difference_CountWentDown_ClampsToZeroRatherThanGoingNegative()
        {
            // A re-imported tournament can rewrite a decklist and make a cumulative count tick down.
            var newer = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 5, wins: 3) };
            var older = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 9, wins: 10) };

            var diff = MetaTierCalculator.Difference(newer, older);

            Assert.That(diff[1].DeckCount, Is.EqualTo(0));
            Assert.That(diff[1].Wins, Is.EqualTo(0));
        }

        [Test]
        public void Difference_LeaderOnlyInOlderSnapshot_IsNotInTheResult()
        {
            var newer = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 10) };
            var older = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 5), [2] = Row(2, 5) };

            var diff = MetaTierCalculator.Difference(newer, older);

            Assert.That(diff.ContainsKey(2), Is.False);
        }

        // ── Delta snapshot selection ─────────────────────────────────────────

        [Test]
        public void SelectDeltaSnapshots_PicksOneAndTwoWindowsBack()
        {
            var snapshots = new[]
            {
                Snapshot(4, "2026-08-17"),
                Snapshot(3, "2026-08-10"),
                Snapshot(2, "2026-08-03"),
                Snapshot(1, "2026-07-27")
            };

            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots(snapshots, deltaWeeks: 1);

            Assert.That(prior!.Id, Is.EqualTo(3));
            Assert.That(priorPrior!.Id, Is.EqualTo(2));
        }

        [Test]
        public void SelectDeltaSnapshots_MissingWeek_SelectsByDateNotByPosition()
        {
            // The week of Aug 10 had no imports, so no snapshot exists for it. Counting back two
            // entries would compare Aug 17 against Aug 03 and call it "one week".
            var snapshots = new[]
            {
                Snapshot(3, "2026-08-17"),
                Snapshot(2, "2026-08-03"),
                Snapshot(1, "2026-07-27")
            };

            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots(snapshots, deltaWeeks: 1);

            Assert.That(prior!.Id, Is.EqualTo(2), "should be the newest snapshot at least a week old");
            Assert.That(priorPrior!.Id, Is.EqualTo(1));
        }

        [Test]
        public void SelectDeltaSnapshots_TwoWeekWindow_SkipsIntermediateWeeks()
        {
            var snapshots = new[]
            {
                Snapshot(5, "2026-08-17"),
                Snapshot(4, "2026-08-10"),
                Snapshot(3, "2026-08-03"),
                Snapshot(2, "2026-07-27"),
                Snapshot(1, "2026-07-20")
            };

            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots(snapshots, deltaWeeks: 2);

            Assert.That(prior!.Id, Is.EqualTo(3));
            Assert.That(priorPrior!.Id, Is.EqualTo(1));
        }

        [Test]
        public void SelectDeltaSnapshots_OnlyTwoWeeksOfHistory_HasNoBaselineForTheOlderWindow()
        {
            var snapshots = new[] { Snapshot(2, "2026-08-17"), Snapshot(1, "2026-08-10") };

            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots(snapshots, deltaWeeks: 1);

            Assert.That(prior!.Id, Is.EqualTo(1));
            Assert.That(priorPrior, Is.Null, "deltas must be suppressed until there are two full windows");
        }

        [Test]
        public void SelectDeltaSnapshots_NewPeriodWithOneSnapshot_ReturnsNothing()
        {
            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots([Snapshot(1, "2026-08-17")], deltaWeeks: 1);

            Assert.That(prior, Is.Null);
            Assert.That(priorPrior, Is.Null);
        }

        [Test]
        public void SelectDeltaSnapshots_EmptyHistory_ReturnsNothing()
        {
            var (prior, priorPrior) = MetaTierCalculator.SelectDeltaSnapshots([], deltaWeeks: 1);

            Assert.That(prior, Is.Null);
            Assert.That(priorPrior, Is.Null);
        }

        // ── Delta application ────────────────────────────────────────────────

        [Test]
        public void ApplyDelta_ShareGrew_ReportsPositivePoints()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 30), [2] = Row(2, 70) };
            var prior = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 10), [2] = Row(2, 90) };

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.MetaShareDeltaPoints, Is.EqualTo(20d).Within(0.001));
        }

        [Test]
        public void ApplyDelta_ThinRecentWindow_SuppressesTheArrow()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, cfg.MinDecksForDelta - 1) };
            var prior = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 50) };

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.MetaShareDeltaPoints, Is.Null);
        }

        [Test]
        public void ApplyDelta_ThinPriorWindow_SuppressesTheArrow()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 50) };
            var prior = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, cfg.MinDecksForDelta - 1) };

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.MetaShareDeltaPoints, Is.Null);
        }

        [Test]
        public void ApplyDelta_AbsentFromPriorWindow_FlagsNewEntryWithoutFakingAnArrow()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 40) };
            var prior = new Dictionary<int, MetaCardSnapshotDBModel>();

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.IsNewEntry, Is.True);
            Assert.That(leader.MetaShareDeltaPoints, Is.Null);
        }

        [Test]
        public void ApplyDelta_NoWindows_LeavesEverythingNull()
        {
            var leader = Leader(cardId: 1);

            MetaTierCalculator.ApplyDelta(leader, null, null, 0, 0, Config());

            Assert.That(leader.MetaShareDeltaPoints, Is.Null);
            Assert.That(leader.WinrateDeltaPoints, Is.Null);
            Assert.That(leader.IsNewEntry, Is.False);
        }

        [Test]
        public void ApplyDelta_WinrateMoved_ReportsPointsDifference()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 40, wins: 60, losses: 40) };  // 60%
            var prior = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 40, wins: 50, losses: 50) };   // 50%

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.WinrateDeltaPoints, Is.EqualTo(10d).Within(0.001));
        }

        [Test]
        public void ApplyDelta_NoGamesInAWindow_LeavesWinrateDeltaNull()
        {
            var cfg = Config();
            var leader = Leader(cardId: 1);
            var recent = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 40, wins: 0, losses: 0) };
            var prior = new Dictionary<int, MetaCardSnapshotDBModel> { [1] = Row(1, 40, wins: 50, losses: 50) };

            MetaTierCalculator.ApplyDelta(leader, recent, prior, 100, 100, cfg);

            Assert.That(leader.WinrateDeltaPoints, Is.Null);
            Assert.That(leader.MetaShareDeltaPoints, Is.Not.Null, "share delta does not depend on games played");
        }
    }
}
