namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateTournamentDto
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int FormatId { get; set; }
        public int? PlayerCount { get; set; }
        public string? SourceUrl { get; set; }
    }
}
