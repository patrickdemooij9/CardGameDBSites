namespace SkytearHorde.Entities.Models.Business
{
    public class CardPriceSyncStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }
        public int SyncedCards { get; set; }
        public string? Error { get; set; }
    }
}
