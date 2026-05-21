namespace SkytearHorde.Entities.Models.Business
{
    public class TournamentMatch
    {
        public Guid Id { get; set; }
        public Guid TournamentEventId { get; set; }
        public Guid TournamentEntrantId { get; set; }
        public int? RoundNumber { get; set; }
        public string? OpponentName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExternalId { get; set; }
    }
}
