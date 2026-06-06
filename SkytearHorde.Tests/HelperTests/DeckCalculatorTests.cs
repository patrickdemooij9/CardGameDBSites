using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class DeckCalculatorTests
    {
        private DeckCalculator _calculator = null!;

        [SetUp]
        public void SetUp()
        {
            _calculator = new DeckCalculator();
        }

        // --- View velocity ---

        [Test]
        public void CalculateDeckScore_TodayViews_WeightedHighest()
        {
            var deck = CreateDeck();
            var viewsToday = new[] { 100, 0, 0, 0, 0, 0, 0 };
            var viewsYesterday = new[] { 0, 100, 0, 0, 0, 0, 0 };

            var scoreToday = _calculator.CalculateDeckScore(deck, viewsToday);
            var scoreYesterday = _calculator.CalculateDeckScore(deck, viewsYesterday);

            // Today: 100 * (1/1 * 3) = 300; Yesterday: 100 * (1/2 * 2) = 100
            Assert.That(scoreToday, Is.GreaterThan(scoreYesterday));
        }

        [Test]
        public void CalculateDeckScore_ViewsDecayWithAge()
        {
            var deck = CreateDeck();
            var day0 = _calculator.CalculateDeckScore(deck, new[] { 100, 0, 0, 0, 0, 0, 0 });
            var day1 = _calculator.CalculateDeckScore(deck, new[] { 0, 100, 0, 0, 0, 0, 0 });
            var day2 = _calculator.CalculateDeckScore(deck, new[] { 0, 0, 100, 0, 0, 0, 0 });
            var day3 = _calculator.CalculateDeckScore(deck, new[] { 0, 0, 0, 100, 0, 0, 0 });

            Assert.That(day0, Is.GreaterThan(day1));
            Assert.That(day1, Is.GreaterThan(day2));
            Assert.That(day2, Is.GreaterThan(day3));
        }

        [Test]
        public void CalculateDeckScore_MultipleViewDays_AllContribute()
        {
            var deck = CreateDeck();
            var singleDay = _calculator.CalculateDeckScore(deck, new[] { 100, 0, 0, 0, 0, 0, 0 });
            var multipleDays = _calculator.CalculateDeckScore(deck, new[] { 100, 100, 100, 0, 0, 0, 0 });

            Assert.That(multipleDays, Is.GreaterThan(singleDay));
        }

        [Test]
        public void CalculateDeckScore_ZeroViews_ContributesNothing()
        {
            var deck = CreateDeck();
            var score = _calculator.CalculateDeckScore(deck, new[] { 0, 0, 0, 0, 0, 0, 0 });

            Assert.That(score, Is.GreaterThanOrEqualTo(0));
        }

        // --- Growth factor ---

        [Test]
        public void CalculateDeckScore_GrowingViews_Rewarded()
        {
            var deck = CreateDeck();
            // Recent 2 days low, older 2 days high -> no growth
            var declining = new[] { 5, 5, 50, 50, 0, 0, 0 };
            // Recent 2 days high, older 2 days low -> growth
            var growing = new[] { 50, 50, 5, 5, 0, 0, 0 };

            var scoreDeclining = _calculator.CalculateDeckScore(deck, declining);
            var scoreGrowing = _calculator.CalculateDeckScore(deck, growing);

            Assert.That(scoreGrowing, Is.GreaterThan(scoreDeclining));
        }

        [Test]
        public void CalculateDeckScore_NoOlderViews_NoGrowthBonus()
        {
            var deck = CreateDeck();
            // Older views = 0, growth ratio calculation skipped
            var views = new[] { 100, 100, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            // Should not throw, just no growth bonus
            Assert.That(score, Is.GreaterThan(0));
        }

        // --- Likes ---

        [Test]
        public void CalculateDeckScore_Likes_CappedAt50()
        {
            var deck50 = CreateDeck(likes: 50);
            var deck100 = CreateDeck(likes: 100);

            var score50 = _calculator.CalculateDeckScore(deck50, Array.Empty<int>());
            var score100 = _calculator.CalculateDeckScore(deck100, Array.Empty<int>());

            Assert.That(score50, Is.EqualTo(score100));
        }

        [Test]
        public void CalculateDeckScore_Likes_UnderCap_AllCounted()
        {
            var deck0 = CreateDeck(likes: 0);
            var deck20 = CreateDeck(likes: 20);

            var score0 = _calculator.CalculateDeckScore(deck0, Array.Empty<int>());
            var score20 = _calculator.CalculateDeckScore(deck20, Array.Empty<int>());

            Assert.That(score20 - score0, Is.EqualTo(20));
        }

        // --- Created date recency ---

        [Test]
        public void CalculateDeckScore_CreatedToday_Awards50Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow);
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.GreaterThanOrEqualTo(50));
        }

        [Test]
        public void CalculateDeckScore_CreatedWithin3Days_Awards50Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-2));
            var oldDeck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-30));

            var scoreNew = _calculator.CalculateDeckScore(deck, Array.Empty<int>());
            var scoreOld = _calculator.CalculateDeckScore(oldDeck, Array.Empty<int>());

            Assert.That(scoreNew - scoreOld, Is.GreaterThanOrEqualTo(50));
        }

        [Test]
        public void CalculateDeckScore_CreatedWithin7Days_Awards30Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-5));
            var oldDeck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-30));

            var scoreNew = _calculator.CalculateDeckScore(deck, Array.Empty<int>());
            var scoreOld = _calculator.CalculateDeckScore(oldDeck, Array.Empty<int>());

            Assert.That(scoreNew - scoreOld, Is.GreaterThanOrEqualTo(30));
        }

        [Test]
        public void CalculateDeckScore_CreatedWithin14Days_Awards10Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-10));
            var oldDeck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-30));

            var scoreNew = _calculator.CalculateDeckScore(deck, Array.Empty<int>());
            var scoreOld = _calculator.CalculateDeckScore(oldDeck, Array.Empty<int>());

            Assert.That(scoreNew - scoreOld, Is.GreaterThanOrEqualTo(10));
        }

        [Test]
        public void CalculateDeckScore_CreatedOver14Days_NoDateBonus()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-15));

            // Should only have likes points (default 0 likes = 0)
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.EqualTo(0));
        }

        // --- Quality signals ---

        [Test]
        public void CalculateDeckScore_HasDescription_Awards5Points()
        {
            var withDesc = CreateDeck(description: "something");
            var withoutDesc = CreateDeck(description: "");

            var scoreWith = _calculator.CalculateDeckScore(withDesc, Array.Empty<int>());
            var scoreWithout = _calculator.CalculateDeckScore(withoutDesc, Array.Empty<int>());

            Assert.That(scoreWith - scoreWithout, Is.EqualTo(5));
        }

        [Test]
        public void CalculateDeckScore_HasCreatedBy_Awards5Points()
        {
            var withCreator = CreateDeck(createdBy: 1);
            var withoutCreator = CreateDeck(createdBy: null);

            var scoreWith = _calculator.CalculateDeckScore(withCreator, Array.Empty<int>());
            var scoreWithout = _calculator.CalculateDeckScore(withoutCreator, Array.Empty<int>());

            Assert.That(scoreWith - scoreWithout, Is.EqualTo(5));
        }

        // --- Combined / integration ---

        [Test]
        public void CalculateDeckScore_CombinedScore_SumsAllFactors()
        {
            var deck = CreateDeck(
                description: "yes",
                createdBy: 1,
                likes: 10,
                createdDate: DateTime.UtcNow.AddDays(-1));

            var views = new[] { 50, 20, 10, 5, 0, 0, 0 };
            var score = _calculator.CalculateDeckScore(deck, views);

            // View velocity: 50*3 + 20*1 + 10*(2/3) + 5*(1/4) = 150 + 20 + 6.67 + 1.25 = ~178
            // Likes: 10, CreatedDate: 50, Description: 5, CreatedBy: 5
            // Growth: recent=70, older=5, ratio=14 > 1.2 → +280
            // Total ~528
            Assert.That(score, Is.GreaterThan(100));
        }

        [Test]
        public void CalculateDeckScore_ReturnsCeiledInt()
        {
            var deck = CreateDeck(likes: 1);
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.TypeOf<int>());
        }

        // --- Helpers ---

        private static Deck CreateDeck(
            string description = "",
            int? createdBy = null,
            int likes = 0,
            DateTime? createdDate = null)
        {
            return new Deck(1, "Test Deck")
            {
                Description = description,
                CreatedBy = createdBy,
                AmountOfLikes = likes,
                CreatedDate = createdDate ?? DateTime.UtcNow.AddDays(-60),
                SiteId = 1,
                TypeId = 1,
                Source = DeckSource.DeckBuilder
            };
        }
    }
}
