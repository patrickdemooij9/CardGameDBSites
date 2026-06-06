namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class ImportTournamentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> MissingCards { get; set; } = [];
    }
}
