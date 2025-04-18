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

            var result = dateTimeUTC.ToString("M");
            if (dateTimeUTC.Year != DateTime.UtcNow.Year)
            {
                result = $"{result} {dateTimeUTC.Year}";
            }
            return result;
        }
    }
}
