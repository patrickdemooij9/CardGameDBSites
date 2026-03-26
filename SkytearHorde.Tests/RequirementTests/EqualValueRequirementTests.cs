using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class EqualValueRequirementTests
    {
        [Test]
        public void IsValid_AllCardsHaveMatchingValue_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Water" } }
                })
            };

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire", "Water" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_OneCardMissingValue_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Other", new[] { "Value" } }
                })
            };

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_CardHasNullAttribute_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1)
            };

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_CardHasNoMatchingAllowedValue_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Earth" } }
                })
            };

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire", "Water" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_EmptyCards_ReturnsTrue()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_MultiValueCardHasOneMatching_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire", "Earth" } }
                })
            };

            var requirement = new EqualValueRequirement("Faction", new[] { "Fire" });
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }
    }
}
