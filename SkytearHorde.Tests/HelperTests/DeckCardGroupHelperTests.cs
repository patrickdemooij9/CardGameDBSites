using SkytearHorde.Business.Helpers;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class DeckCardGroupHelperTests
    {
        [Test]
        public void GroupByCardCount_SplitsAtThreshold()
        {
            var items = new[]
            {
                CreateGroup(3),
                CreateGroup(3),
                CreateGroup(3)
            };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 5).ToList();

            // The helper yields a group each time threshold is met, plus a final empty yield
            // Group 1: first two groups (3+3=6 >= 5) -> yielded
            // Then 3 cards remain, threshold not met -> final yield with last group + empty
            Assert.That(result[0].Sum(g => g.Cards.Length), Is.EqualTo(6));
        }

        [Test]
        public void GroupByCardCount_EmptyGroupsSkipped()
        {
            var items = new[]
            {
                CreateGroup(2),
                CreateGroup(0),
                CreateGroup(2)
            };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 5).ToList();

            // 2 + 0 + 2 = 4 < 5, all in one group (empty group skipped), then final empty yield
            // First group has 2 items (empty skipped), second group is empty
            Assert.That(result[0], Has.Length.EqualTo(2));
        }

        [Test]
        public void GroupByCardCount_SingleLargeGroup_AlsoYieldsFinalEmpty()
        {
            var items = new[] { CreateGroup(10) };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 5).ToList();

            // 10 >= 5: first group yielded, then final empty group yielded
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0][0].Cards.Length, Is.EqualTo(10));
            Assert.That(result[1], Has.Length.EqualTo(0));
        }

        [Test]
        public void GroupByCardCount_ExactThreshold_BoundaryBehavior()
        {
            var items = new[]
            {
                CreateGroup(5),
                CreateGroup(1)
            };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 5).ToList();

            // First group hits threshold (5 >= 5), gets yielded
            // Second group (1) doesn't hit, goes to final yield
            Assert.That(result[0].Sum(g => g.Cards.Length), Is.EqualTo(5));
            Assert.That(result[1].Sum(g => g.Cards.Length), Is.EqualTo(1));
        }

        [Test]
        public void GroupByCardCount_EmptyInput_ReturnsSingleEmptyGroup()
        {
            var items = Array.Empty<DeckCardGroupViewModel>();

            var result = DeckCardGroupHelper.GroupByCardCount(items, 5).ToList();

            // Even with empty input, final yield return executes once with empty list
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Has.Length.EqualTo(0));
        }

        [Test]
        public void GroupByCardCount_ThresholdOf1_EachItemYieldsImmediately()
        {
            var items = new[]
            {
                CreateGroup(1),
                CreateGroup(1),
                CreateGroup(1)
            };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 1).ToList();

            // Each item hits threshold (1 >= 1): yields 3 groups, plus final empty = 4
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0][0].Cards.Length, Is.EqualTo(1));
            Assert.That(result[1][0].Cards.Length, Is.EqualTo(1));
            Assert.That(result[2][0].Cards.Length, Is.EqualTo(1));
            Assert.That(result[3], Has.Length.EqualTo(0));
        }

        [Test]
        public void GroupByCardCount_LargeThreshold_AllInOneGroup()
        {
            var items = new[]
            {
                CreateGroup(2),
                CreateGroup(3),
                CreateGroup(1)
            };

            var result = DeckCardGroupHelper.GroupByCardCount(items, 100).ToList();

            // 6 < 100, all items in single group from final yield
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Has.Length.EqualTo(3));
        }

        private static DeckCardGroupViewModel CreateGroup(int cardCount)
        {
            var cards = Enumerable.Range(1, cardCount)
                .Select(i => new Entities.Models.Business.Card(i)
                {
                    DisplayName = $"Card {i}",
                    SetId = 1,
                    SetName = "Test",
                    UrlSegment = $"card-{i}"
                })
                .ToArray();

            return new DeckCardGroupViewModel
            {
                Deck = new Entities.Models.Business.Deck(1, "Test") { SiteId = 1, TypeId = 1, Source = DeckSource.DeckBuilder },
                Group = null!,
                Cards = cards,
                SquadSettings = null!
            };
        }
    }
}
