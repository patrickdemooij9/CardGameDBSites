using SkytearHorde.Entities.Models.ViewModels;

namespace CardGameDBSites.API.Models.Tournaments
{
    public class TournamentWinningDeckApiModel
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime TournamentDateUtc { get; set; }
        public string? ExternalUrl { get; set; }
        public string? PlayerName { get; set; }
        public required DeckApiModel Deck { get; set; }
    }
}
