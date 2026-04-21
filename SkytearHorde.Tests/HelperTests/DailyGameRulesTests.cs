using SkytearHorde.Business.Services;

namespace SkytearHorde.Tests.HelperTests
{
    public class DailyGameRulesTests
    {
        [TestCase(true, 1, 5, "Solved")]
        [TestCase(false, 4, 5, "InProgress")]
        [TestCase(false, 5, 5, "Failed")]
        public void ResolveStatus_ReturnsExpectedState(bool isCorrect, int attemptsUsed, int maxAttempts, string expected)
        {
            var result = DailyGameRules.ResolveStatus(isCorrect, attemptsUsed, maxAttempts);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CompareValues_NumericHint_Works()
        {
            Assert.That(DailyGameRules.CompareValues(["5"], ["3"]), Is.EqualTo("higher"));
            Assert.That(DailyGameRules.CompareValues(["3"], ["5"]), Is.EqualTo("lower"));
            Assert.That(DailyGameRules.CompareValues(["3"], ["3"]), Is.EqualTo("exact"));
        }

        [Test]
        public void CompareValues_SetComparison_Works()
        {
            Assert.That(DailyGameRules.CompareValues(["Hero", "Jedi"], ["Jedi", "Hero"]), Is.EqualTo("exact"));
            Assert.That(DailyGameRules.CompareValues(["Hero", "Jedi"], ["Hero", "Pilot"]), Is.EqualTo("partial"));
            Assert.That(DailyGameRules.CompareValues(["Hero"], ["Villain"]), Is.EqualTo("none"));
        }

        [Test]
        public void Rank_OrdersSolvedThenAttemptsThenTime()
        {
            var items = new[]
            {
                new Item { Name = "A", Solved = false, Attempts = 3, Time = 10 },
                new Item { Name = "B", Solved = true, Attempts = 3, Time = 20 },
                new Item { Name = "C", Solved = true, Attempts = 2, Time = 30 },
                new Item { Name = "D", Solved = true, Attempts = 3, Time = 10 },
            };

            var ranked = DailyGameRules.Rank(items, it => it.Solved, it => it.Attempts, it => it.Time)
                .Select(it => it.Name)
                .ToArray();

            Assert.That(ranked, Is.EqualTo(new[] { "C", "D", "B", "A" }));
        }

        private class Item
        {
            public required string Name { get; set; }
            public bool Solved { get; set; }
            public int Attempts { get; set; }
            public int Time { get; set; }
        }
    }
}
