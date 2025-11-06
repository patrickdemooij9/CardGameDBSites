namespace CardGameDBSites.API.Models
{
    public class OverviewFilterApiModel
    {
        public required string Alias { get; set; }
        public required string DisplayName { get; set; }
        public bool IsInline { get; set; }

        public OverviewFilterOptionApiModel[] Options { get; set; } = [];
        public bool AutoFillValues { get; set; }
    }
}
