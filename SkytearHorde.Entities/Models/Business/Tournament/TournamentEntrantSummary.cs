namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class TournamentEntrantSummary
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public string PlayerName { get; set; }
        public int Placement { get; set; }
        public int TournamentDeckId { get; set; }
        public string? DeckName { get; set; }
        public int? LeaderCardId { get; set; }
    }
}
