using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Tests.Utils;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Tests.ServiceTests
{
    [TestFixture]
    public class DeckLegalityServiceTests
    {
        private const int StandardTypeId = 1;
        private const int TwinSunsTypeId = 2;

        private static Card CreateCard(int id, params int[] nonLegalDeckTypes)
        {
            var card = CardTestHelper.CreateCard(id);
            card.NonLegalDeckTypes = nonLegalDeckTypes;
            return card;
        }

        [Test]
        public void GetIllegalCardIds_CardNotLegalForType_ReturnsOnlyThatCard()
        {
            var cards = new[] { CreateCard(1, StandardTypeId), CreateCard(2), CreateCard(3, TwinSunsTypeId) };

            var result = DeckLegalityService.GetIllegalCardIds(cards, StandardTypeId);

            Assert.That(result, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void GetIllegalCardIds_SameBaseCardTwice_ReturnsItOnce()
        {
            var cards = new[] { CreateCard(1, StandardTypeId), CreateCard(1, StandardTypeId) };

            var result = DeckLegalityService.GetIllegalCardIds(cards, StandardTypeId);

            Assert.That(result, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void GetIllegalCardIds_CardWithoutLegalityData_IsTreatedAsLegal()
        {
            var card = CardTestHelper.CreateCard(1);
            card.NonLegalDeckTypes = null!;

            var result = DeckLegalityService.GetIllegalCardIds([card], StandardTypeId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ContainsIllegalCard_OneIllegalCard_ReturnsTrue()
        {
            var cards = new Dictionary<int, Card> { [1] = CreateCard(1), [2] = CreateCard(2, StandardTypeId) };

            var result = DeckLegalityService.ContainsIllegalCard([1, 2], cards, StandardTypeId);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ContainsIllegalCard_IllegalForOtherTypeOnly_ReturnsFalse()
        {
            var cards = new Dictionary<int, Card> { [1] = CreateCard(1, TwinSunsTypeId) };

            var result = DeckLegalityService.ContainsIllegalCard([1], cards, StandardTypeId);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ContainsIllegalCard_UnknownCardId_IsIgnored()
        {
            var cards = new Dictionary<int, Card> { [1] = CreateCard(1) };

            var result = DeckLegalityService.ContainsIllegalCard([1, 99], cards, StandardTypeId);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ComputeNonLegalDeckTypes_OnlyPrintingIsInIllegalSet_IsIllegal()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([], [[StandardTypeId]]);

            Assert.That(result, Is.EqualTo(new[] { StandardTypeId }));
        }

        [Test]
        public void ComputeNonLegalDeckTypes_ReprintedInLegalSet_StaysLegal()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([], [[StandardTypeId], []]);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ComputeNonLegalDeckTypes_AllPrintingsIllegal_IsIllegal()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([], [[StandardTypeId, TwinSunsTypeId], [StandardTypeId]]);

            Assert.That(result, Is.EqualTo(new[] { StandardTypeId }));
        }

        [Test]
        public void ComputeNonLegalDeckTypes_SuspendedCardInLegalSet_IsIllegal()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([StandardTypeId], [[]]);

            Assert.That(result, Is.EqualTo(new[] { StandardTypeId }));
        }

        [Test]
        public void ComputeNonLegalDeckTypes_SuspendedAndInIllegalSet_ReturnsTypeOnce()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([StandardTypeId], [[StandardTypeId]]);

            Assert.That(result, Is.EqualTo(new[] { StandardTypeId }));
        }

        [Test]
        public void ComputeNonLegalDeckTypes_NoPrintings_ReturnsCardOwnTypes()
        {
            var result = DeckLegalityService.ComputeNonLegalDeckTypes([TwinSunsTypeId], []);

            Assert.That(result, Is.EqualTo(new[] { TwinSunsTypeId }));
        }

        [Test]
        public void FindChanges_LegalDeckGetsIllegalCard_BecomesIllegal()
        {
            var versions = new[] { CreateVersion(1, 10, isLegal: true, published: true, 100) };

            var (becameLegal, becameIllegal, illegalDecks) = DeckLegalityService.FindChanges(versions, [100]);

            Assert.That(becameLegal, Is.Empty);
            Assert.That(becameIllegal, Is.EqualTo(new[] { 1 }));
            Assert.That(illegalDecks, Is.EqualTo(1));
        }

        [Test]
        public void FindChanges_IllegalDeckWithoutIllegalCards_BecomesLegal()
        {
            var versions = new[] { CreateVersion(1, 10, isLegal: false, published: true, 100) };

            var (becameLegal, becameIllegal, illegalDecks) = DeckLegalityService.FindChanges(versions, [200]);

            Assert.That(becameLegal, Is.EqualTo(new[] { 1 }));
            Assert.That(becameIllegal, Is.Empty);
            Assert.That(illegalDecks, Is.EqualTo(0));
        }

        [Test]
        public void FindChanges_FlagAlreadyCorrect_ChangesNothingButCountsIllegalDecks()
        {
            var versions = new[]
            {
                CreateVersion(1, 10, isLegal: true, published: true, 100),
                CreateVersion(2, 20, isLegal: false, published: true, 200)
            };

            var (becameLegal, becameIllegal, illegalDecks) = DeckLegalityService.FindChanges(versions, [200]);

            Assert.That(becameLegal, Is.Empty);
            Assert.That(becameIllegal, Is.Empty);
            Assert.That(illegalDecks, Is.EqualTo(1));
        }

        [Test]
        public void FindChanges_DraftHasIllegalCard_PublishedVersionDecides()
        {
            var versions = new[]
            {
                CreateVersion(1, 10, isLegal: true, published: true, 100),
                CreateVersion(1, 11, isLegal: true, published: false, 200)
            };

            var (_, becameIllegal, illegalDecks) = DeckLegalityService.FindChanges(versions, [200]);

            Assert.That(becameIllegal, Is.Empty);
            Assert.That(illegalDecks, Is.EqualTo(0));
        }

        [Test]
        public void FindChanges_NeverPublishedDeck_DraftDecides()
        {
            var versions = new[] { CreateVersion(1, 10, isLegal: true, published: false, 200) };

            var (_, becameIllegal, _) = DeckLegalityService.FindChanges(versions, [200]);

            Assert.That(becameIllegal, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void HasBasePrintingIn_BasePrintingInSet_ReturnsTrue()
        {
            var card = CreateCard(1);
            card.VariantReferences = [new CardVariantReference(null, 10, 5)];

            Assert.That(DeckLegalityService.HasBasePrintingIn(card, 5), Is.True);
        }

        [Test]
        public void HasBasePrintingIn_OnlyVariantPrintingInSet_ReturnsFalse()
        {
            var card = CreateCard(1);
            card.VariantReferences = [new CardVariantReference(3, 10, 5)];

            Assert.That(DeckLegalityService.HasBasePrintingIn(card, 5), Is.False);
        }

        private static DeckLegalityVersion CreateVersion(int deckId, int versionId, bool isLegal, bool published, params int[] cardIds)
        {
            return new DeckLegalityVersion
            {
                DeckId = deckId,
                VersionId = versionId,
                IsLegal = isLegal,
                Published = published,
                CardIds = cardIds
            };
        }
    }
}
