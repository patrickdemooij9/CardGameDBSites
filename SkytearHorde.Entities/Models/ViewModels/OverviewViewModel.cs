using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels.DataSources;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class OverviewViewModel
    {
        public IOverviewDataSource DataSource { get; }
        public OverviewDataSourceConfig Config { get; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public int PageId { get; set; }
        public int PageNumber { get; set; }

        public OverviewViewModel(IOverviewDataSource dataSource, OverviewDataSourceConfig config)
        {
            DataSource = dataSource;
            Config = config;
        }
    }
}
