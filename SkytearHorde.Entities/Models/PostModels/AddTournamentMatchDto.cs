namespace SkytearHorde.Entities.Models.PostModels
{
    public class AddTournamentMatchDto
    {
        public int? RoundNumber { get; set; }
        public string? OpponentName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}
