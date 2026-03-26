using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class ChildOfSquadRequirementTests
    {
        [Test]
        public void IsValid_AllChildrenAllowed_ReturnsTrue()
        {
            var parent = CardTestHelper.CreateCardWithChildren(1, new[] { 10, 20, 30 });
            var cards = new[]
            {
                CardTestHelper.CreateCard(10),
                CardTestHelper.CreateCard(20)
            };

            var requirement = new ChildOfSquadRequirement(parent);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_SomeChildrenNotAllowed_ReturnsFalse()
        {
            var parent = CardTestHelper.CreateCardWithChildren(1, new[] { 10, 20 });
            var cards = new[]
            {
                CardTestHelper.CreateCard(10),
                CardTestHelper.CreateCard(30)
            };

            var requirement = new ChildOfSquadRequirement(parent);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_EmptyCards_ReturnsTrue()
        {
            var parent = CardTestHelper.CreateCardWithChildren(1, new[] { 10, 20 });
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new ChildOfSquadRequirement(parent);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_EmptyAllowedChildren_NoCardsAllowed_ReturnsFalse()
        {
            var parent = CardTestHelper.CreateCardWithChildren(1, Array.Empty<int>());
            var cards = new[]
            {
                CardTestHelper.CreateCard(10)
            };

            var requirement = new ChildOfSquadRequirement(parent);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_SingleChildAllowed_ReturnsTrue()
        {
            var parent = CardTestHelper.CreateCardWithChildren(1, new[] { 42 });
            var cards = new[] { CardTestHelper.CreateCard(42) };

            var requirement = new ChildOfSquadRequirement(parent);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }
    }
}
