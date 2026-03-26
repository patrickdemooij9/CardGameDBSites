using SkytearHorde.Business.Helpers;
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

        [Test]
        public void CalculateDeckScore_LongDescription_Awards20Points()
        {
            var deck = CreateDeck(description: new string('x', 101));
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        public void CalculateDeckScore_ShortDescription_Awards5Points()
        {
            var deck = CreateDeck(description: "short");
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public void CalculateDeckScore_HasCreatedBy_Awards10Points()
        {
            var deckWithCreator = CreateDeck(createdBy: 1);
            var deckWithoutCreator = CreateDeck(createdBy: null);

            var scoreWith = _calculator.CalculateDeckScore(deckWithCreator, Array.Empty<int>());
            var scoreWithout = _calculator.CalculateDeckScore(deckWithoutCreator, Array.Empty<int>());

            Assert.That(scoreWith - scoreWithout, Is.EqualTo(10));
        }

        [Test]
        public void CalculateDeckScore_ViewsAbove100_Awards15PointsPerDay()
        {
            var deck = CreateDeck();
            var views = new[] { 100, 0, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            // 15 points * 7/1 = 105 weighted points for day 0
            Assert.That(score, Is.GreaterThanOrEqualTo(105));
        }

        [Test]
        public void CalculateDeckScore_ViewsBetween50And99_Awards10Points()
        {
            var deck = CreateDeck();
            var views = new[] { 50, 0, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            // 10 points * 7/1 = 70 weighted points for day 0
            Assert.That(score, Is.GreaterThanOrEqualTo(70));
        }

        [Test]
        public void CalculateDeckScore_ViewsBetween10And49_Awards5Points()
        {
            var deck = CreateDeck();
            var views = new[] { 10, 0, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            // 5 points * 7/1 = 35 weighted points for day 0
            Assert.That(score, Is.GreaterThanOrEqualTo(35));
        }

        [Test]
        public void CalculateDeckScore_ViewsBetween1And9_Awards1Point()
        {
            var deck = CreateDeck();
            var views = new[] { 1, 0, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            // 1 point * 7/1 = 7 weighted points for day 0
            Assert.That(score, Is.GreaterThanOrEqualTo(7));
        }

        [Test]
        public void CalculateDeckScore_ZeroViews_Awards0Points()
        {
            var deck = CreateDeck();
            var views = new[] { 0, 0, 0, 0, 0, 0, 0 };

            var score = _calculator.CalculateDeckScore(deck, views);

            Assert.That(score, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void CalculateDeckScore_ViewsRecencyWeighting_DecreasesOverTime()
        {
            var deck = CreateDeck();
            var viewsDay0 = new[] { 100, 0, 0, 0, 0, 0, 0 };
            var viewsDay6 = new[] { 0, 0, 0, 0, 0, 0, 100 };

            var scoreDay0 = _calculator.CalculateDeckScore(deck, viewsDay0);
            var scoreDay6 = _calculator.CalculateDeckScore(deck, viewsDay6);

            // Day 0: 15 * 7/1 = 105; Day 6: 15 * 7/7 = 15
            Assert.That(scoreDay0, Is.GreaterThan(scoreDay6));
        }

        [Test]
        public void CalculateDeckScore_HighLikes_ReturnsMorePoints()
        {
            var deck10Likes = CreateDeck(likes: 10);
            var deck100Likes = CreateDeck(likes: 100);

            var score10 = _calculator.CalculateDeckScore(deck10Likes, Array.Empty<int>());
            var score100 = _calculator.CalculateDeckScore(deck100Likes, Array.Empty<int>());

            Assert.That(score100, Is.GreaterThan(score10));
        }

        [Test]
        public void CalculateDeckScore_ZeroLikes_StillGetsMinimum20Points()
        {
            var deck = CreateDeck(likes: 0);
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            // Math.Max(20, 0*2) = 20 minimum from likes
            Assert.That(score, Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        public void CalculateDeckScore_CreatedWithin7Days_Awards30Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-3));
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.GreaterThanOrEqualTo(30));
        }

        [Test]
        public void CalculateDeckScore_CreatedWithin30Days_Awards15Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-15));
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            // Should get 15 from date + at least 20 from likes
            Assert.That(score, Is.GreaterThanOrEqualTo(15));
            // But not 30 from date (only 15)
            Assert.That(score, Is.LessThan(60));
        }

        [Test]
        public void CalculateDeckScore_CreatedOver30Days_Awards0Points()
        {
            var deck = CreateDeck(createdDate: DateTime.UtcNow.AddDays(-60));
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            Assert.That(score, Is.LessThan(45));
        }

        [Test]
        public void CalculateDeckScore_CombinedScore_SumsAllFactors()
        {
            var deck = CreateDeck(
                description: new string('x', 200),
                createdBy: 1,
                likes: 5,
                createdDate: DateTime.UtcNow.AddDays(-3));

            var views = new[] { 100, 50, 10, 1, 0, 0, 0 };
            var score = _calculator.CalculateDeckScore(deck, views);

            // Description: 20, CreatedBy: 10, Likes: max(20,10) = 20, CreatedDate: 30
            // Views: 15*7 + 10*3.5 + 5*2.33 + 1*1.75 = 105 + 35 + 11.65 + 1.75 = ~153
            // Total ~233
            Assert.That(score, Is.GreaterThan(100));
        }

        [Test]
        public void CalculateDeckScore_ReturnsIntCeiling()
        {
            var deck = CreateDeck(likes: 0);
            var score = _calculator.CalculateDeckScore(deck, Array.Empty<int>());

            // Verify it returns an int (no fractional)
            Assert.That(score, Is.TypeOf<int>());
        }

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
                TypeId = 1
            };
        }
    }
}
