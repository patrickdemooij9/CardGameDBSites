namespace AdServer.Models
{
    public class AdMetric
    {
        public int AdId { get; set; }
        public DateOnly Date { get; set; }

        public int Impressions { get; set; }
        public int TrackedImpressions { get; set; }

        public int Clicks { get; set; }
        public int TrackedClicks { get; set; }

        public AdMetric(int adId, DateOnly date)
        {
            AdId = adId;
            Date = date;
        }
    }
}
