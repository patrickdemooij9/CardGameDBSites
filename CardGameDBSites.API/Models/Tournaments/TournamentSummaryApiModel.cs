namespace CardGameDBSites.API.Models.Tournaments
{
    public class TournamentSummaryApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateUtc { get; set; }
        public string Type { get; set; }
        public string? ExternalUrl { get; set; }
        public int PlayerCount { get; set; }
        public TournamentEntrantApiModel? Winner { get; set; }
    }
}
