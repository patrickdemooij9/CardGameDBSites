using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Card = SkytearHorde.Entities.Models.Business.Card;
using CardAttribute = SkytearHorde.Entities.Models.Business.CardAttribute;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class SameValueRequirementTests
    {
        [Test]
        public void TestSameValueRequirementSucceeding()
        {
            var cards = new List<Card>();
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test2"} }
            }));
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test1", "Test2"} }
            }));
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test2"} }
            }));

            var requirement = new SameValueRequirement("Test");
            var result = requirement.IsValid(cards.ToArray());

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSameValueRequirementFailing()
        {
            var cards = new List<Card>();
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test1, Test2"} }
            }));
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test1"} }
            }));
            cards.Add(CreateCard(new Dictionary<string, string[]>
            {
                {"Test", new []{"Test2"} }
            }));

            var requirement = new SameValueRequirement("Test");
            var result = requirement.IsValid(cards.ToArray());

            Assert.That(result, Is.False);
        }

        private Card CreateCard(Dictionary<string, string[]> values)
        {
            var card = new Card(1)
            {
                DisplayName = "",
                SetId = 1,
                SetName = "",
                UrlSegment = ""
            };
            var blockListItems = new List<BlockListItem>();
            foreach(var value in values)
            {
                var attributeMock = new ModelsBuilderMock<MultiTextAbilityValue>();
                var ability = new CardAttribute
                {
                    Name = value.Key
                };
                card.Attributes.Add(ability, attributeMock.Mock);
            }

            return card;
        }
    }
}
