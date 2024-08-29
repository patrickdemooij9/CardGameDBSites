using SkytearHorde.Entities.Models.ViewModels.DataSources;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class OverviewDataViewModel
    {
        public int PageId { get; set; }
        public int? PageNumber { get; set; }
        public OverviewDataSourceConfig Config { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }

        public OverviewDataViewModel(OverviewDataSourceConfig config)
        {
            Config = config;
        }
    }
}
