namespace CardGameDBSites.API.Models.Settings
{
    public class SetOverviewSettingsApiModel
    {
        public OverviewFilterApiModel[] Filters { get; set; } = [];
        public OverviewSortOptionApiModel[] SortOptions { get; set; } = [];
    }
}
