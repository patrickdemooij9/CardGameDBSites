using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class SizeSquadRequirementTests
    {
        [Test]
        public void IsValid_WithinMinMax_ReturnsTrue()
        {
            var cards = CreateCards(3);
            var requirement = new SizeSquadRequirement(2, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_BelowMin_ReturnsFalse()
        {
            var cards = CreateCards(1);
            var requirement = new SizeSquadRequirement(2, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_AboveMax_ReturnsFalse()
        {
            var cards = CreateCards(6);
            var requirement = new SizeSquadRequirement(2, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_ExactMin_ReturnsTrue()
        {
            var cards = CreateCards(2);
            var requirement = new SizeSquadRequirement(2, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_ExactMax_ReturnsTrue()
        {
            var cards = CreateCards(5);
            var requirement = new SizeSquadRequirement(2, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoMinSet_ReturnsTrueWhenBelowMax()
        {
            var cards = CreateCards(1);
            var requirement = new SizeSquadRequirement(null, 5);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoMaxSet_ReturnsTrueWhenAboveMin()
        {
            var cards = CreateCards(100);
            var requirement = new SizeSquadRequirement(2, null);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoMinNoMax_AlwaysTrue()
        {
            var cards = CreateCards(0);
            var requirement = new SizeSquadRequirement(null, null);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_EmptyCards_WithMin_ReturnsFalse()
        {
            var cards = CreateCards(0);
            var requirement = new SizeSquadRequirement(1, null);

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        private static Entities.Models.Business.Card[] CreateCards(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Entities.Models.Business.Card(i)
                {
                    DisplayName = $"Card {i}",
                    SetId = 1,
                    SetName = "Test",
                    UrlSegment = $"card-{i}"
                })
                .ToArray();
        }
    }
}
