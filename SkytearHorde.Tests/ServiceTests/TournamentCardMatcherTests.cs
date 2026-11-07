using SkytearHorde.Business.Tournaments;
using SkytearHorde.Tests.Utils;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Tests.ServiceTests
{
    [TestFixture]
    public class TournamentCardMatcherTests
    {
        private const int StandardTypeId = 1;

        private static Card CreateCard(int id, string name, params int[] nonLegalDeckTypes)
        {
            var card = CardTestHelper.CreateCard(id);
            card.DisplayName = name;
            card.NonLegalDeckTypes = nonLegalDeckTypes;
            return card;
        }

        [Test]
        public void Find_PartialNameOfOtherCard_DoesNotMatchIt()
        {
            var matcher = new TournamentCardMatcher([CreateCard(1, "Death Watch Loyalist")], StandardTypeId);

            Assert.That(matcher.Find("Watch This", null), Is.Null);
        }

        [Test]
        public void Find_ExactNameNextToLongerName_ReturnsExactCard()
        {
            var matcher = new TournamentCardMatcher([CreateCard(1, "Freetown Backup"), CreateCard(2, "Freetown")], StandardTypeId);

            Assert.That(matcher.Find("Freetown", null)?.BaseId, Is.EqualTo(2));
        }

        [Test]
        public void Find_NameWithSubtitle_JoinsThemLikeTheDisplayName()
        {
            var matcher = new TournamentCardMatcher([CreateCard(1, "Boba Fett, Feared Bounty Hunter")], StandardTypeId);

            Assert.That(matcher.Find("Boba Fett", "Feared Bounty Hunter")?.BaseId, Is.EqualTo(1));
        }

        [Test]
        public void Find_EmptySubtitle_IsIgnored()
        {
            var matcher = new TournamentCardMatcher([CreateCard(1, "Commandeer")], StandardTypeId);

            Assert.That(matcher.Find("Commandeer", "")?.BaseId, Is.EqualTo(1));
        }

        [Test]
        public void Find_DifferentCasingAndPunctuation_StillMatches()
        {
            var matcher = new TournamentCardMatcher([CreateCard(1, "Sabé, Queen's shadow")], StandardTypeId);

            Assert.That(matcher.Find("Sabé", "Queen’s Shadow")?.BaseId, Is.EqualTo(1));
        }

        [Test]
        public void Find_SameNameTwice_PrefersTheLegalCard()
        {
            var cards = new[] { CreateCard(1, "Mechanize", StandardTypeId), CreateCard(2, "Mechanize") };
            var matcher = new TournamentCardMatcher(cards, StandardTypeId);

            Assert.That(matcher.Find("Mechanize", null)?.BaseId, Is.EqualTo(2));
        }

        [Test]
        public void Find_SameNameBothIllegal_StillReturnsACard()
        {
            var cards = new[] { CreateCard(1, "Mechanize", StandardTypeId), CreateCard(2, "Mechanize", StandardTypeId) };
            var matcher = new TournamentCardMatcher(cards, StandardTypeId);

            Assert.That(matcher.Find("Mechanize", null), Is.Not.Null);
        }
    }
}
