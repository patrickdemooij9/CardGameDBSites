namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class TournamentEntrant
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public string PlayerName { get; set; }
        public int Placement { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public string? ExternalId { get; set; }
        public string Source { get; set; }
        public int TournamentDeckId { get; set; }
    }
}
