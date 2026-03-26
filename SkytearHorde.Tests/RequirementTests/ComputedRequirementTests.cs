using Moq;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class ComputedRequirementTests
    {
        [Test]
        public void IsValid_SumEqual_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "3" } },
                    { "Defense", new[] { "3" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.Equal);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_SumNotEqual_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "5" } },
                    { "Defense", new[] { "3" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.Equal);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_SumHigherThan_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "5" } },
                    { "Defense", new[] { "3" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.HigherThan);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_SumLowerThan_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "2" } },
                    { "Defense", new[] { "3" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.LowerThan);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NonNumericValue_TreatedAsZero()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "abc" } },
                    { "Defense", new[] { "0" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.Equal);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NullAttributeValue_TreatedAsZero()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Defense", new[] { "0" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Sum, "Attack",
                ComputedType.Sum, "Defense",
                ComputedComparisonType.Equal);

            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_CountEqual_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "1" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Attack", new[] { "2" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Defense", new[] { "1" } }
                }),
                CardTestHelper.CreateCard(4, new Dictionary<string, string[]>
                {
                    { "Defense", new[] { "2" } }
                }),
                CardTestHelper.CreateCard(5, new Dictionary<string, string[]>
                {
                    { "Defense", new[] { "3" } }
                })
            };

            var config = CreateConfig(
                ComputedType.Count, "Attack",
                ComputedType.Count, "Defense",
                ComputedComparisonType.Equal);

            // Count is based on matchingFirstAbility which selects cards where
            // FirstAbilityRequirement is valid - but then uses matchingFirstAbility for both counts
            // The implementation uses matchingFirstAbility for both computed values
            // So this tests the count behavior
            var requirement = new ComputedRequirement(config);
            var result = requirement.IsValid(cards);

            // Both count from matchingFirstAbility cards' Attack and Defense values
            Assert.That(result, Is.True);
        }

        private static ComputedRequirementConfig CreateConfig(
            ComputedType firstType, string firstValue,
            ComputedType secondType, string secondValue,
            ComputedComparisonType comparison)
        {
            var allCardsCondition = new Mock<ISquadRequirement>();
            allCardsCondition.Setup(r => r.IsValid(It.IsAny<Entities.Models.Business.Card[]>()))
                .Returns(true);

            return new ComputedRequirementConfig
            {
                FirstAbilityRequirement = allCardsCondition.Object,
                FirstAbilityType = firstType,
                FirstAbilityValue = firstValue,
                SecondAbilityRequirement = allCardsCondition.Object,
                SecondAbilityType = secondType,
                SecondAbilityValue = secondValue,
                Comparison = comparison
            };
        }
    }
}
