using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Entities.Models.ViewModels.DataSources
{
    public class OverviewDataSourceConfig
    {
        public bool WhiteBackground { get; set; }
        public bool HideSearch { get; set; }
        public bool HideFilters { get; set; }

        public FilterViewModel[] Filters { get; set; }
        public FilterViewModel[] InternalFilters { get; set; }
        public SortByViewModel[] Sortings { get; set; }
        public OverviewViewType[] AvailableViews { get; set; }

        public int? PageSize { get; set; }

        public OverviewDataSourceConfig()
        {
            Filters = [];
            Sortings = [];
            AvailableViews = [];
            InternalFilters = [];
        }
    }
}
