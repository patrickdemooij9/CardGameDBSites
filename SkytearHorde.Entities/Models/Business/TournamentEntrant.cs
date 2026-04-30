namespace SkytearHorde.Entities.Models.Business
{
    public class TournamentEntrant
    {
        public Guid Id { get; set; }
        public Guid TournamentEventId { get; set; }
        public string PlayerName { get; set; }
        public int? Placement { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Draws { get; set; }
        public int? DeckId { get; set; }
    }
}
