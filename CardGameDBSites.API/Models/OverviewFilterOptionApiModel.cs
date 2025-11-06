namespace CardGameDBSites.API.Models
{
    public class OverviewFilterOptionApiModel
    {
        public required string DisplayName { get; set; }
        public required string Value { get; set; }
        public string? IconUrl { get; set; }
    }
}
