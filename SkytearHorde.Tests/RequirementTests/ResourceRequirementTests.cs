using Moq;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class ResourceRequirementTests
    {
        [Test]
        public void IsValid_ResourcesSatisfyRequirements_ReturnsTrue()
        {
            var mainCard = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy" } }
            });
            var otherCard = CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Energy" } }
            });
            var allCards = new[] { mainCard, otherCard };

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, false);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_InsufficientResources_ReturnsFalse()
        {
            var mainCard = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy" } }
            });
            var otherCard = CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Mana" } }
            });
            var allCards = new[] { mainCard, otherCard };

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, false);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_NoMainCards_ReturnsFalse()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Energy" } }
            });

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, false);

            var result = requirement.IsValid(new[] { card });

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_OtherCardHasNoRequiredAttribute_ReturnsFalse()
        {
            var mainCard = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy" } }
            });
            var otherCard = CardTestHelper.CreateCard(2);
            var allCards = new[] { mainCard, otherCard };

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, false);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_SingleResourceMode_InsufficientQuantity_ReturnsFalse()
        {
            var mainCard = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy" } }
            });
            var otherCard = CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Energy", "Energy" } }
            });
            var allCards = new[] { mainCard, otherCard };

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, true);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_SingleResourceMode_SufficientQuantity_ReturnsTrue()
        {
            var mainCard = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy", "Energy" } }
            });
            var otherCard = CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Energy", "Energy" } }
            });
            var allCards = new[] { mainCard, otherCard };

            var mainCondition = CreateCardCondition("Provides", "Energy");
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, true);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_MultipleMainCards_AllResourcesAvailable()
        {
            var main1 = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Energy" } }
            });
            var main2 = CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
            {
                { "Provides", new[] { "Mana" } }
            });
            var other = CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
            {
                { "Requires", new[] { "Energy" } }
            });
            var allCards = new[] { main1, main2, other };

            var mainCondition = CreateCardCondition("Provides", null);
            var requirement = new ResourceRequirement("Provides", "Requires", new[] { mainCondition }, false);

            var result = requirement.IsValid(allCards);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Creates a condition that checks if a single card has a specific attribute,
        /// optionally filtering for a specific value. Passing null for expectedValue
        /// means any value is accepted.
        /// </summary>
        private static ISquadRequirement CreateCardCondition(string attributeKey, string? expectedValue)
        {
            var mock = new Mock<ISquadRequirement>();
            mock.Setup(r => r.IsValid(It.IsAny<Card[]>()))
                .Returns<Card[]>(cards =>
                {
                    foreach (var card in cards)
                    {
                        var values = card.GetMultipleCardAttributeValue(attributeKey);
                        if (values == null) return false;
                        if (expectedValue != null && !values.Contains(expectedValue)) return false;
                    }
                    return true;
                });
            return mock.Object;
        }
    }
}
