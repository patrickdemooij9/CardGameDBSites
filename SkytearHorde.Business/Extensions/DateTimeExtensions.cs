namespace SkytearHorde.Business.Extensions
{
    public static class DateTimeExtensions
    {
        public static string DateOffsetHumanReadable(this DateTime dateTimeUTC)
        {
            var days = (DateTime.UtcNow - dateTimeUTC).Days;
            if (days == 0) return "Today";
            if (days == 1) return "Yesterday";
            if (days <= 30) return $"{days} days ago";
            if (days <= 365) return dateTimeUTC.ToString("M");
            return $"{dateTimeUTC.ToString("M")} {dateTimeUTC.Year}";
        }
    }
}
