using SkytearHorde.Entities.Models.Business.Tournament;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class MetaWeekTests
    {
        // 2026-08-17 is a Monday.
        [TestCase("2026-08-17", "2026-08-17")]   // Monday -> itself
        [TestCase("2026-08-18", "2026-08-17")]   // Tuesday
        [TestCase("2026-08-23", "2026-08-17")]   // Sunday, the last day of that week
        [TestCase("2026-08-24", "2026-08-24")]   // next Monday rolls over
        public void StartOfWeek_ReturnsMondayOfThatWeek(string input, string expected)
        {
            var result = MetaWeek.StartOfWeek(DateTime.Parse(input));

            Assert.That(result, Is.EqualTo(DateTime.Parse(expected)));
        }

        [Test]
        public void StartOfWeek_StripsTimeOfDay()
        {
            var result = MetaWeek.StartOfWeek(new DateTime(2026, 8, 19, 23, 45, 12, DateTimeKind.Utc));

            Assert.That(result, Is.EqualTo(new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void StartOfWeek_WeekSpanningYearBoundary_ReturnsMondayInPreviousYear()
        {
            // 2027-01-01 is a Friday; its week starts Monday 2026-12-28.
            var result = MetaWeek.StartOfWeek(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.That(result, Is.EqualTo(new DateTime(2026, 12, 28, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void StartOfWeek_ResultIsUtc()
        {
            var result = MetaWeek.StartOfWeek(new DateTime(2026, 8, 19));

            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void EndOfWeek_IsSevenDaysAfterStart()
        {
            var start = new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.That(MetaWeek.EndOfWeek(start), Is.EqualTo(new DateTime(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void WeeksBetween_SpansEveryWeekInclusive()
        {
            var weeks = MetaWeek.WeeksBetween(
                new DateTime(2026, 8, 19, 0, 0, 0, DateTimeKind.Utc),   // week of the 17th
                new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc))    // week of Aug 31
                .ToArray();

            Assert.That(weeks, Is.EqualTo(new[]
            {
                new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc)
            }));
        }

        [Test]
        public void WeeksBetween_SameWeek_ReturnsSingleEntry()
        {
            var weeks = MetaWeek.WeeksBetween(
                new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 23, 0, 0, 0, DateTimeKind.Utc))
                .ToArray();

            Assert.That(weeks, Has.Length.EqualTo(1));
        }

        [Test]
        public void WeeksBetween_RangeRunsBackwards_ReturnsEmpty()
        {
            var weeks = MetaWeek.WeeksBetween(
                new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc))
                .ToArray();

            Assert.That(weeks, Is.Empty);
        }
    }
}
