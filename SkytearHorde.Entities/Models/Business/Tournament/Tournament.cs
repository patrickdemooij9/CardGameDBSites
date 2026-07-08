namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FormatId { get; set; }
        public string Type { get; set; }
        public DateTime DateUtc { get; set; }
        public string Source { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ExternalId { get; set; }
        public int SiteId { get; set; }
        public int? PeriodId { get; set; }
    }
}
