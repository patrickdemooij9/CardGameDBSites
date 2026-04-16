using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Models.Database;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class CardPriceDeltaCalculatorTests
    {
        // --- CalculateDelta (new insert) ---

        [Test]
        public void CalculateDelta_NoPreviousRecord_ReturnsZero()
        {
            var delta = CardPriceDeltaCalculator.CalculateDelta(25.0, null);

            Assert.That(delta, Is.EqualTo(0.0));
        }

        [Test]
        public void CalculateDelta_PriceIncreased_ReturnsPositiveDelta()
        {
            var previous = CreateRecord(mainPrice: 25.0);

            var delta = CardPriceDeltaCalculator.CalculateDelta(26.0, previous);

            Assert.That(delta, Is.EqualTo(1.0));
        }

        [Test]
        public void CalculateDelta_PriceDecreased_ReturnsNegativeDelta()
        {
            var previous = CreateRecord(mainPrice: 25.0);

            var delta = CardPriceDeltaCalculator.CalculateDelta(24.0, previous);

            Assert.That(delta, Is.EqualTo(-1.0));
        }

        [Test]
        public void CalculateDelta_PriceUnchanged_ReturnsZeroDelta()
        {
            var previous = CreateRecord(mainPrice: 25.0);

            var delta = CardPriceDeltaCalculator.CalculateDelta(25.0, previous);

            Assert.That(delta, Is.EqualTo(0.0));
        }

        [Test]
        public void CalculateDelta_FromZeroPreviousPrice_ReturnsFullNewPrice()
        {
            var previous = CreateRecord(mainPrice: 0.0);

            var delta = CardPriceDeltaCalculator.CalculateDelta(10.0, previous);

            Assert.That(delta, Is.EqualTo(10.0));
        }

        // --- RecalculateDelta (same-day update) ---

        [Test]
        public void RecalculateDelta_PriceIncreasedFromPredecessor_ReturnsPositiveDelta()
        {
            // Predecessor was $20, this record was $25 (delta=5), now updating to $26
            var existing = CreateRecord(mainPrice: 25.0, delta: 5.0);

            var delta = CardPriceDeltaCalculator.RecalculateDelta(26.0, existing);

            // Predecessor = 25 - 5 = 20; new delta = 26 - 20 = 6
            Assert.That(delta, Is.EqualTo(6.0));
        }

        [Test]
        public void RecalculateDelta_PriceDecreasedFromPredecessor_ReturnsNegativeDelta()
        {
            // Predecessor was $25, this record was $20 (delta=-5), now updating to $18
            var existing = CreateRecord(mainPrice: 20.0, delta: -5.0);

            var delta = CardPriceDeltaCalculator.RecalculateDelta(18.0, existing);

            // Predecessor = 20 - (-5) = 25; new delta = 18 - 25 = -7
            Assert.That(delta, Is.EqualTo(-7.0));
        }

        [Test]
        public void RecalculateDelta_SamePriceAsPredecessor_ReturnsZeroDelta()
        {
            // Predecessor was $25, this record was $30 (delta=5), now updating back to $25
            var existing = CreateRecord(mainPrice: 30.0, delta: 5.0);

            var delta = CardPriceDeltaCalculator.RecalculateDelta(25.0, existing);

            // Predecessor = 30 - 5 = 25; new delta = 25 - 25 = 0
            Assert.That(delta, Is.EqualTo(0.0));
        }

        [Test]
        public void RecalculateDelta_FirstEverRecord_ZeroExistingDelta_ReturnsCorrectDelta()
        {
            // First record (no predecessor), mainPrice=$20, delta=0; now update to $22
            var existing = CreateRecord(mainPrice: 20.0, delta: 0.0);

            var delta = CardPriceDeltaCalculator.RecalculateDelta(22.0, existing);

            // Predecessor = 20 - 0 = 20; new delta = 22 - 20 = 2
            Assert.That(delta, Is.EqualTo(2.0));
        }

        // --- Helpers ---

        private static CardPriceRecordDBModel CreateRecord(double mainPrice, double delta = 0.0)
        {
            return new CardPriceRecordDBModel
            {
                CardId = 1,
                MainPrice = mainPrice,
                Delta = delta,
                DateUtc = DateTime.UtcNow,
                IsLatest = true
            };
        }
    }
}
