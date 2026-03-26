using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class RequiredCardRequirementTests
    {
        [Test]
        public void IsValid_RequiredCardPresent_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1),
                CardTestHelper.CreateCard(42),
                CardTestHelper.CreateCard(3)
            };

            var requirement = new RequiredCardRequirement(42);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_RequiredCardAbsent_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1),
                CardTestHelper.CreateCard(2),
                CardTestHelper.CreateCard(3)
            };

            var requirement = new RequiredCardRequirement(42);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_EmptyCards_ReturnsFalse()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new RequiredCardRequirement(42);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_SingleCardMatches_ReturnsTrue()
        {
            var cards = new[] { CardTestHelper.CreateCard(42) };

            var requirement = new RequiredCardRequirement(42);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }
    }
}
