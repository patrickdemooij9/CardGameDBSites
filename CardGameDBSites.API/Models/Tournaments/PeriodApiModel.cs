namespace CardGameDBSites.API.Models.Tournaments
{
    public class PeriodApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartingDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public bool IsCurrent { get; set; }
    }
}
