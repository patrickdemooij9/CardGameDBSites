namespace SkytearHorde.Entities.Models.PostModels
{
    public class AddEntrantDto
    {
        public string PlayerName { get; set; }
        public int? Placement { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Draws { get; set; }
        public int? DeckId { get; set; }
    }
}
