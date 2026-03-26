using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class SameValueRequirementTests
    {
        [Test]
        public void TestSameValueRequirementSucceeding()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test2" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test1", "Test2" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test2" } }
                })
            };

            var requirement = new SameValueRequirement("Test");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSameValueRequirementFailing()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test1, Test2" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test1" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Test", new[] { "Test2" } }
                })
            };

            var requirement = new SameValueRequirement("Test");
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }
    }
}
