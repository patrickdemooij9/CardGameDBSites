namespace SkytearHorde.Entities.Models.PostModels
{
    public class AddEntrantDto
    {
        public required string PlayerName { get; set; }
        public int? Placement { get; set; }
        public int? DeckId { get; set; }
        public List<AddTournamentMatchDto>? Matches { get; set; }
    }
}
