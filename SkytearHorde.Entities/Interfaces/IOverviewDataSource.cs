using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Entities.Models.ViewModels.DataSources;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.Entities.Interfaces
{
    public interface IOverviewDataSource
    {
        OverviewDataSourceKey SourceKey { get; }
        string ViewComponentName { get; }

        OverviewDataSourceConfig GetConfig(IPublishedContent page);
    }
}
