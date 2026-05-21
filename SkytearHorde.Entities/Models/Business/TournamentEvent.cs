namespace SkytearHorde.Entities.Models.Business
{
    public class TournamentEvent
    {
        public Guid Id { get; set; }
        public int SiteId { get; set; }
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public int FormatId { get; set; }
        public string? FormatDisplayName { get; set; }
        public int? PlayerCount { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExternalId { get; set; }
        public List<TournamentEntrant> Entrants { get; set; } = new();
    }
}
