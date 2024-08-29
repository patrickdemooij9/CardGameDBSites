namespace AdServer.Models
{
    /// <summary>
    /// The full data point. Still needs to be processed by the server in batches
    /// </summary>
    public class AdMetricRawData
    {
        public int AdId { get; set; }
        public bool Impression { get; set; }
        public bool Click { get; set; }
        public bool Tracked { get; set; }
    }
}
