using Moq;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Tests.Utils
{
    public static class CardTestHelper
    {
        public static Card CreateCard(int id, Dictionary<string, string[]>? attributes = null)
        {
            var card = new Card(id)
            {
                DisplayName = $"Card {id}",
                SetId = 1,
                SetName = "Test Set",
                UrlSegment = $"card-{id}"
            };

            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    var mock = new Mock<IAbilityValue>();
                    mock.Setup(a => a.GetValues()).Returns(attr.Value);
                    card.Attributes[attr.Key] = mock.Object;
                }
            }

            return card;
        }

        public static Card CreateCard(int id, string setName, Dictionary<string, string[]>? attributes = null)
        {
            var card = CreateCard(id, attributes);
            card.SetName = setName;
            return card;
        }

        public static Card CreateCardWithChildren(int id, int[] allowedChildren, int maxChildren = 0)
        {
            var card = CreateCard(id);
            card.AllowedChildren = allowedChildren;
            card.MaxChildren = maxChildren;
            return card;
        }
    }
}
