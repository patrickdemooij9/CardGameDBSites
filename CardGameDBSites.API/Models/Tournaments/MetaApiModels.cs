namespace CardGameDBSites.API.Models.Tournaments
{
    public class MetaWinningDeckApiModel
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime TournamentDateUtc { get; set; }
        public string? ExternalUrl { get; set; }
        public string? PlayerName { get; set; }
        public int DeckId { get; set; }
        public string? DeckName { get; set; }
        public string? LeaderName { get; set; }
    }

    public class MetaLeaderApiModel
    {
        public string LeaderName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Top8Count { get; set; }
    }

    public class MetaPopularCardApiModel
    {
        public string CardName { get; set; } = string.Empty;
        public int Percentage { get; set; }
    }
}
