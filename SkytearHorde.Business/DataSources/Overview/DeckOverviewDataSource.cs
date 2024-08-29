using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Entities.Models.ViewModels.DataSources;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.Business.DataSources.Overview
{
    public class DeckOverviewDataSource : IOverviewDataSource
    {
        private readonly SettingsService _settingsService;
        private readonly MemberInfoService _memberInfoService;

        public OverviewDataSourceKey SourceKey => OverviewDataSourceKey.DeckOverview;

        public string ViewComponentName => "DeckOverviewData";

        public DeckOverviewDataSource(SettingsService settingsService, MemberInfoService memberInfoService)
        {
            _settingsService = settingsService;
            _memberInfoService = memberInfoService;
        }

        public OverviewDataSourceConfig GetConfig(IPublishedContent page)
        {
            var sortings = new List<SortByViewModel>
            {
                new("Popular", "popular"),
                new("Newest", "newest"),
            };
            if (_settingsService.GetCollectionSettings().AllowCardCollecting && _memberInfoService.GetMemberInfo()?.IsLoggedIn is true)
            {
                sortings.Add(new SortByViewModel("Collection", "collection"));
            }

            return new OverviewDataSourceConfig()
            {
                HideSearch = true,

                AvailableViews = new[] { OverviewViewType.Images },
                Sortings = sortings.ToArray(),
                Filters = new FilterViewModel[]
                {
                    new("Deck includes cards", "Cards")
                    {
                        ApiEndpoint = "Test"
                    }
                }
            };
        }
    }
}
