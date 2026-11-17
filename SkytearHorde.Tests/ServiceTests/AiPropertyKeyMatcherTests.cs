using SkytearHorde.Business.Services;

namespace SkytearHorde.Tests.ServiceTests
{
    [TestFixture]
    public class AiPropertyKeyMatcherTests
    {
        private static readonly string[] Aliases = ["Name", "Collection Icons_multiple", "Pip Requirement_multiple", "Shard Cost"];

        [TestCase("Collection Icons_multiple")]
        [TestCase("CollectionIcons_multiple")]
        [TestCase("Collection_Icons_multiple")]
        [TestCase("collection icons")]
        [TestCase("Collection_Icons")]
        public void FindAlias_RewrittenKey_ReturnsConfiguredAlias(string aiKey)
        {
            Assert.That(AiPropertyKeyMatcher.FindAlias(aiKey, Aliases), Is.EqualTo("Collection Icons_multiple"));
        }

        [Test]
        public void FindAlias_SingleValueAlias_Matches()
        {
            Assert.That(AiPropertyKeyMatcher.FindAlias("ShardCost", Aliases), Is.EqualTo("Shard Cost"));
        }

        [Test]
        public void FindAlias_UnknownKey_ReturnsNull()
        {
            Assert.That(AiPropertyKeyMatcher.FindAlias("Flavor Text", Aliases), Is.Null);
        }
    }
}
