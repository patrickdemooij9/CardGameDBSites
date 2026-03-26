using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class NotEqualValueRequirementTests
    {
        [Test]
        public void IsValid_NoCardsHaveForbiddenValues_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Water" } }
                })
            };

            var requirement = new NotEqualValueRequirement("Faction", new[] { "Earth" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_OneCardHasForbiddenValue_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Earth" } }
                })
            };

            var requirement = new NotEqualValueRequirement("Faction", new[] { "Earth", "Shadow" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_CardHasNullAttribute_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1)
            };

            var requirement = new NotEqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_EmptyCards_ReturnsTrue()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new NotEqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_MultiValueCardHasForbiddenValue_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire", "Earth" } }
                })
            };

            var requirement = new NotEqualValueRequirement("Faction", new[] { "Earth" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }
    }
}
