namespace CardGameDBSites.API.Models.Meta
{
    public class MetaTierListApiModel
    {
        public int PeriodId { get; set; }
        public string PeriodName { get; set; } = string.Empty;

        public DateTime? FirstEventUtc { get; set; }
        public DateTime? LastEventUtc { get; set; }

        /// <summary>Newest tournament date, not the time of the request.</summary>
        public DateTime? LastUpdatedUtc { get; set; }

        public int TotalDecks { get; set; }
        public int TotalEvents { get; set; }
        public int TotalEntrants { get; set; }
        public int EntrantsWithDeck { get; set; }

        public bool DeltasAvailable { get; set; }
        public string? DeltasUnavailableReason { get; set; }
        public DateTime? DeltaComparedToUtc { get; set; }
        public int DeltaWeeks { get; set; }

        public int MinDecks { get; set; }
        public int MinEvents { get; set; }

        public MetaTierLeaderApiModel[] Leaders { get; set; } = [];
    }
}
