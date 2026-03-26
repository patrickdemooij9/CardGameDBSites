using SkytearHorde.Business.Extensions;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void DateOffsetHumanReadable_Today_ReturnsToday()
        {
            var date = DateTime.UtcNow;

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("Today"));
        }

        [Test]
        public void DateOffsetHumanReadable_Yesterday_ReturnsYesterday()
        {
            var date = DateTime.UtcNow.AddDays(-1);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("Yesterday"));
        }

        [Test]
        public void DateOffsetHumanReadable_2DaysAgo_Returns2DaysAgo()
        {
            var date = DateTime.UtcNow.AddDays(-2);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("2 days ago"));
        }

        [Test]
        public void DateOffsetHumanReadable_15DaysAgo_Returns15DaysAgo()
        {
            var date = DateTime.UtcNow.AddDays(-15);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("15 days ago"));
        }

        [Test]
        public void DateOffsetHumanReadable_30DaysAgo_Returns30DaysAgo()
        {
            var date = DateTime.UtcNow.AddDays(-30);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("30 days ago"));
        }

        [Test]
        public void DateOffsetHumanReadable_OlderThan30Days_ReturnsFormattedDate()
        {
            var date = DateTime.UtcNow.AddDays(-31);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Does.Not.Contain("days ago"));
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void DateOffsetHumanReadable_DifferentYear_IncludesYear()
        {
            var date = new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Does.Contain("2020"));
        }

        [Test]
        public void DateOffsetHumanReadable_SameYear_NoYearShown()
        {
            var now = DateTime.UtcNow;
            // Use a date in the same year but > 30 days ago
            var date = new DateTime(now.Year, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            if ((now - date).TotalDays <= 30)
            {
                // Skip if it happens to be within 30 days
                Assert.Pass("Same year test skipped - too close to Jan 15");
                return;
            }

            var result = date.DateOffsetHumanReadable();

            // If same year, year should not be appended
            if (date.Year == now.Year)
            {
                Assert.That(result, Does.Not.Contain(now.Year.ToString()));
            }
        }

        [Test]
        public void DateOffsetHumanReadable_Exactly7DaysAgo_Returns7DaysAgo()
        {
            var date = DateTime.UtcNow.AddDays(-7);

            var result = date.DateOffsetHumanReadable();

            Assert.That(result, Is.EqualTo("7 days ago"));
        }
    }
}
