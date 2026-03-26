using Moq;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class ConditionalRequirementTests
    {
        [Test]
        public void IsValid_ConditionMetAndRequirementPasses_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Type", new[] { "Leader" } }
                })
            };

            var condition = CreateConditionThatMatches("Type", "Leader");
            var innerRequirement = CreateRequirementThatReturns(true);

            var requirement = new ConditionalRequirement(
                new[] { condition },
                new[] { innerRequirement });

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_ConditionMetButRequirementFails_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Type", new[] { "Leader" } }
                })
            };

            var condition = CreateConditionThatMatches("Type", "Leader");
            var innerRequirement = CreateRequirementThatReturns(false);

            var requirement = new ConditionalRequirement(
                new[] { condition },
                new[] { innerRequirement });

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_NoCardsMatchCondition_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Type", new[] { "Unit" } }
                })
            };

            var condition = CreateConditionThatMatches("Type", "Leader");
            var innerRequirement = CreateRequirementThatReturns(false);

            var requirement = new ConditionalRequirement(
                new[] { condition },
                new[] { innerRequirement });

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_SomeCardsMatchCondition_OnlyMatchedCardsChecked()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Type", new[] { "Leader" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Type", new[] { "Unit" } }
                })
            };

            var condition = CreateConditionThatMatches("Type", "Leader");
            var innerRequirement = CreateRequirementThatReturns(true);

            var requirement = new ConditionalRequirement(
                new[] { condition },
                new[] { innerRequirement });

            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        private static ISquadRequirement CreateConditionThatMatches(string key, string value)
        {
            var mock = new Mock<ISquadRequirement>();
            mock.Setup(r => r.IsValid(It.IsAny<Entities.Models.Business.Card[]>()))
                .Returns<Entities.Models.Business.Card[]>(cards =>
                    cards.Any(c => c.GetMultipleCardAttributeValue(key)?.Contains(value) == true));
            return mock.Object;
        }

        private static ISquadRequirement CreateRequirementThatReturns(bool result)
        {
            var mock = new Mock<ISquadRequirement>();
            mock.Setup(r => r.IsValid(It.IsAny<Entities.Models.Business.Card[]>()))
                .Returns(result);
            return mock.Object;
        }
    }
}
