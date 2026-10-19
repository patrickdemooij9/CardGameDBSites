namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class PriceSyncStatusApiModel
    {
        public required bool IsRunning { get; set; }
        public DateTime? StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }
        public required int SyncedCards { get; set; }
        public string? Error { get; set; }
        public DateTime? LatestPriceDate { get; set; }
    }
}
