namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class TournamentEntrant
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public required string PlayerName { get; set; }
        public required int Placement { get; set; }
        public required int Wins { get; set; }
        public required int Losses { get; set; }
        public required int Draws { get; set; }
        public string? ExternalId { get; set; }
        public required string Source { get; set; }
        public int TournamentDeckId { get; set; }
    }
}
