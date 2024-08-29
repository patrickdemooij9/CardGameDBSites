namespace SkytearHorde.Business.Helpers
{
    public class DateHelper
    {
        public static TimeSpan GetToNextTime(int hour)
        {
            var date = DateTime.UtcNow.Date.AddHours(hour);
            if (date < DateTime.UtcNow)
                date = date.AddDays(1);

            return date.Subtract(DateTime.UtcNow);
        }
    }
}
