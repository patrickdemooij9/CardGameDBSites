using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Entities.Models.ViewModels.DataSources;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SkytearHorde.Business.DataSources.Overview
{
    public class CollectionSetDetailDataSource : IOverviewDataSource
    {
        private readonly ISiteService _siteService;
        private readonly CardService _cardService;
        private readonly MemberInfoService _memberInfoService;

        public OverviewDataSourceKey SourceKey => OverviewDataSourceKey.CollectionSetDetail;

        public string ViewComponentName => "CardOverviewData";

        public CollectionSetDetailDataSource(ISiteService siteService, CardService cardService, MemberInfoService memberInfoService)
        {
            _siteService = siteService;
            _cardService = cardService;
            _memberInfoService = memberInfoService;
        }

        public OverviewDataSourceConfig GetConfig(IPublishedContent page)
        {
            var overview = _siteService.GetSetOverview();
            if (overview is null) return new OverviewDataSourceConfig();

            var set = page as Set;
            if (set is null) return new OverviewDataSourceConfig();

            var filters = new List<FilterViewModel>();

            foreach (var filter in overview.SetFilters.ToItems<OverviewFilter>())
            {
                var abilityName = filter.IsExpansionFilter ? "Set Name" : filter.Ability!.Name;

                var filterModel = new FilterViewModel(filter.DisplayName!, abilityName)
                {
                    IsInline = filter.IsInline
                };
                if (filter.AutoFillValues)
                {
                    foreach (var value in _cardService.GetCardValues(filter.Ability.Name).OrderBy(it => it))
                    {
                        filterModel.Items.Add(new FilterItemViewModel(value, value));
                    }
                }
                else
                {
                    foreach (var item in filter.Items.ToItems<OverviewFilterItem>())
                    {
                        var filterItem = new FilterItemViewModel(item.DisplayName!, item.Value!);
                        if (item.Icon != null)
                        {
                            filterItem.IconUrl = item.Icon.Url();
                        }
                        filterModel.Items.Add(filterItem);
                    }
                }
                filters.Add(filterModel);
            }

            var sortings = overview.SetSortings.ToItems<SortingItem>().Select(it => new SortByViewModel(it.DisplayName!, it.Value!)).ToList();

            var isLoggedIn = _memberInfoService.GetMemberInfo()?.IsLoggedIn is true;
            if (isLoggedIn)
            {
                filters.Add(new FilterViewModel("Collection", "collection")
                {
                    Items = new List<FilterItemViewModel>
                {
                    new FilterItemViewModel("In collection", "inCollection"),
                    new FilterItemViewModel("No copies", "none"),
                    new FilterItemViewModel("Missing", "missing")
                }
                });

                sortings.Add(new SortByViewModel("Collection", "collection"));
            }

            var variantTypeId = (set.MainVariantType as Variant)?.InternalID ?? 0;
            var config = new CardOverviewDataSourceConfig()
            {
                WhiteBackground = true,
                Filters = [.. filters],
                InternalFilters =
                [
                    new FilterViewModel("Expansion", "SetId")
                    {
                        Items = new List<FilterItemViewModel>
                        {
                            { new FilterItemViewModel("Set", set.Id.ToString()){ IsChecked = true } }
                        }
                    }
                ],
                Sortings = [.. sortings],
                AvailableViews = [OverviewViewType.Images, OverviewViewType.Rows],
                IsValid = (card) => card.SetId == set.Id,
                VariantTypeId = variantTypeId
            };
            foreach (var ability in overview.SetPropertiesToShow?.OfType<CardAttribute>() ?? [])
            {
                config.AttributesToShow.Add(ability.DisplayName, ability.Name);
            }
            config.AttributesToShow.Add("Collection", "");

            return config;
        }
    }
}
