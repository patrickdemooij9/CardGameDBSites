using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class UniqueValueRequirementTests
    {
        [Test]
        public void IsValid_AllUniqueValues_ReturnsTrue()
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
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Earth" } }
                })
            };

            var requirement = new UniqueValueRequirement("Faction");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_DuplicateValuePresent_ReturnsFalse()
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
                })
            };

            var requirement = new UniqueValueRequirement("Faction");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_CardHasNullAttribute_SkipsCard()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire" } }
                })
            };

            var requirement = new UniqueValueRequirement("Faction");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_EmptyCards_ReturnsTrue()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new UniqueValueRequirement("Faction");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_DuplicateAcrossMultiValueCards_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Fire", "Water" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Faction", new[] { "Earth", "Fire" } }
                })
            };

            var requirement = new UniqueValueRequirement("Faction");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }
    }
}
