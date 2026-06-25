namespace SkytearHorde.Entities.Models.Business.Tournament
{
    /// <summary>
    /// A deck that won a recent tournament, with its derived leader.
    /// </summary>
    public class MetaWinningDeck
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

    /// <summary>
    /// Aggregated performance of a single leader over a time window.
    /// </summary>
    public class MetaLeaderStat
    {
        public string LeaderName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Top8Count { get; set; }
    }

    /// <summary>
    /// A card and the percentage of winning decks it appears in.
    /// </summary>
    public class MetaPopularCard
    {
        public string CardName { get; set; } = string.Empty;
        public int Percentage { get; set; }
    }
}
