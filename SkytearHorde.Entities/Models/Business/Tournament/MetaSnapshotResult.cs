namespace SkytearHorde.Entities.Models.Business.Tournament
{
    /// <summary>
    /// Summary of a snapshot (re)computation, returned by the recompute service/endpoint so a caller
    /// can confirm what was written without querying the tables directly.
    /// </summary>
    public class MetaSnapshotResult
    {
        public int SnapshotId { get; set; }
        public int PeriodId { get; set; }
        public int TotalDecks { get; set; }
        public int CardRowCount { get; set; }
    }
}
