using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Tests.ModelTests
{
    [TestFixture]
    public class DeckTests
    {
        [Test]
        public void CalculateCollection_FullyOwned_Returns100Percent()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 2),
                new DeckCard(2, 0, 0, 1)
            });

            var collection = new[]
            {
                new CollectionCardItem { CardId = 1, Amount = 2 },
                new CollectionCardItem { CardId = 2, Amount = 1 }
            };

            var result = deck.CalculateCollection(collection);

            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void CalculateCollection_NothingOwned_Returns0Percent()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 2),
                new DeckCard(2, 0, 0, 1)
            });

            var collection = Array.Empty<CollectionCardItem>();

            var result = deck.CalculateCollection(collection);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateCollection_PartiallyOwned_ReturnsPartialPercent()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 2),
                new DeckCard(2, 0, 0, 2)
            });

            var collection = new[]
            {
                new CollectionCardItem { CardId = 1, Amount = 2 },
                new CollectionCardItem { CardId = 2, Amount = 1 }
            };

            var result = deck.CalculateCollection(collection);

            // Owned: 2 (card 1 full) + 1 (card 2 partial) = 3 out of 4 = 75%
            Assert.That(result, Is.EqualTo(75));
        }

        [Test]
        public void CalculateCollection_DuplicateCollectionCards_SumsAmount()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 3)
            });

            var collection = new[]
            {
                new CollectionCardItem { CardId = 1, Amount = 1 },
                new CollectionCardItem { CardId = 1, Amount = 2 }
            };

            var result = deck.CalculateCollection(collection);

            // 1 + 2 = 3 owned out of 3 needed = 100%
            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void CalculateCollection_CollectionExceedsNeeded_CappedAtNeeded()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 1)
            });

            var collection = new[]
            {
                new CollectionCardItem { CardId = 1, Amount = 10 }
            };

            var result = deck.CalculateCollection(collection);

            // Min(1, 10) = 1 out of 1 = 100%
            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void GetDeckCard_Found_ReturnsDeckCard()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(42, 0, 0, 1)
            });

            var result = deck.GetDeckCard(42);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CardId, Is.EqualTo(42));
        }

        [Test]
        public void GetDeckCard_NotFound_ReturnsNull()
        {
            var deck = CreateDeck(new[]
            {
                new DeckCard(1, 0, 0, 1)
            });

            var result = deck.GetDeckCard(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDeckCard_EmptyDeck_ReturnsNull()
        {
            var deck = CreateDeck(Array.Empty<DeckCard>());

            var result = deck.GetDeckCard(1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Constructor_InitializesDefaultValues()
        {
            var deck = new Deck("Test Deck") { SiteId = 1, TypeId = 1 };

            Assert.That(deck.Name, Is.EqualTo("Test Deck"));
            Assert.That(deck.Cards, Is.Not.Null);
            Assert.That(deck.Cards, Is.Empty);
            Assert.That(deck.Score, Is.EqualTo(0));
            Assert.That(deck.AmountOfLikes, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithId_SetsId()
        {
            var deck = new Deck(42, "Test Deck") { SiteId = 1, TypeId = 1 };

            Assert.That(deck.Id, Is.EqualTo(42));
            Assert.That(deck.Name, Is.EqualTo("Test Deck"));
        }

        private static Deck CreateDeck(DeckCard[] cards)
        {
            return new Deck(1, "Test Deck")
            {
                Cards = cards.ToList(),
                SiteId = 1,
                TypeId = 1
            };
        }
    }
}

