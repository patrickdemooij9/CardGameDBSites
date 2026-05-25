namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class TournamentMatch
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public int? Entrant1Id { get; set; }
        public int? Entrant2Id { get; set; }
        public int WinnerEntrantId { get; set; }
        public int GamesWonP1 { get; set; }
        public int GamesWonP2 { get; set; }
    }
}
