namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public static class MetaWeek
    {
        public static DateTime StartOfWeek(DateTime utc)
        {
            var date = utc.Date;
            var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
            return DateTime.SpecifyKind(date.AddDays(-daysSinceMonday), DateTimeKind.Utc);
        }

        public static DateTime EndOfWeek(DateTime weekStartUtc) => weekStartUtc.AddDays(7);

        public static IEnumerable<DateTime> WeeksBetween(DateTime fromUtc, DateTime toUtc)
        {
            var current = StartOfWeek(fromUtc);
            var last = StartOfWeek(toUtc);
            while (current <= last)
            {
                yield return current;
                current = current.AddDays(7);
            }
        }
    }
}
