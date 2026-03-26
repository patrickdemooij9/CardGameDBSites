using Moq;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Tests.Utils;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Tests.ModelTests
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void GetMultipleCardAttributeValue_FoundAttribute_ReturnsValues()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Faction", new[] { "Fire", "Water" } }
            });

            var result = card.GetMultipleCardAttributeValue("Faction");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(new[] { "Fire", "Water" }));
        }

        [Test]
        public void GetMultipleCardAttributeValue_MissingAttribute_ReturnsNull()
        {
            var card = CardTestHelper.CreateCard(1);

            var result = card.GetMultipleCardAttributeValue("Faction");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMultipleCardAttributeValue_SetName_ReturnsSetName()
        {
            var card = CardTestHelper.CreateCard(1, "My Set");

            var result = card.GetMultipleCardAttributeValue("Set Name");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(new[] { "My Set" }));
        }

        [Test]
        public void GetMultipleCardAttributeValue_DifferentAttributeName_FindsCorrect()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Cost", new[] { "5" } },
                { "Type", new[] { "Unit" } }
            });

            var result = card.GetMultipleCardAttributeValue("Type");

            Assert.That(result, Is.EqualTo(new[] { "Unit" }));
        }

        [Test]
        public void GetAmount_ValidAmount_ReturnsParsedInt()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Amount", new[] { "3" } }
            });

            var result = card.GetAmount();

            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetAmount_MissingAmount_ReturnsZero()
        {
            var card = CardTestHelper.CreateCard(1);

            var result = card.GetAmount();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetAmount_NonNumericAmount_ReturnsZero()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Amount", new[] { "abc" } }
            });

            var result = card.GetAmount();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetCost_ValidCost_ReturnsParsedInt()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Cost", new[] { "7" } }
            });

            var result = card.GetCost();

            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        public void GetCost_MissingCost_ReturnsZero()
        {
            var card = CardTestHelper.CreateCard(1);

            var result = card.GetCost();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetCost_NonNumericCost_ReturnsZero()
        {
            var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
            {
                { "Cost", new[] { "free" } }
            });

            var result = card.GetCost();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void Clone_CopiesKeyProperties()
        {
            var card = CardTestHelper.CreateCard(42, "My Set");
            card.VariantTypeId = 5;

            var clone = card.Clone();

            Assert.That(clone.BaseId, Is.EqualTo(42));
            Assert.That(clone.DisplayName, Is.EqualTo("Card 42"));
            Assert.That(clone.SetId, Is.EqualTo(1));
            Assert.That(clone.SetName, Is.EqualTo("My Set"));
            Assert.That(clone.UrlSegment, Is.EqualTo("card-42"));
            Assert.That(clone.VariantTypeId, Is.EqualTo(5));
        }

        [Test]
        public void Clone_CreatesIndependentInstance()
        {
            var card = CardTestHelper.CreateCard(1);
            card.DisplayName = "Original";

            var clone = card.Clone();
            clone.DisplayName = "Modified";

            Assert.That(card.DisplayName, Is.EqualTo("Original"));
            Assert.That(clone.DisplayName, Is.EqualTo("Modified"));
        }
    }
}
