namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateTournamentDto
    {
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public int FormatId { get; set; }
        public int? PlayerCount { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceType { get; set; }
    }
}
