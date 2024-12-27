using DeviceDetectorNET.Class.Device;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Entities.Models.ViewModels.DataSources;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.DataSources.Overview
{
    public class CardOverviewDataSource : IOverviewDataSource
    {
        private readonly CardService _cardService;
        private readonly MemberInfoService _memberInfoService;

        public OverviewDataSourceKey SourceKey => OverviewDataSourceKey.CardOverview;
        public string ViewComponentName => "CardOverviewData";

        public CardOverviewDataSource(CardService cardService, MemberInfoService memberInfoService)
        {
            _cardService = cardService;
            _memberInfoService = memberInfoService;
        }

        public OverviewViewType[] AvailableViews()
        {
            return new[] { OverviewViewType.Images, OverviewViewType.Rows };
        }

        public OverviewDataSourceConfig GetConfig(IPublishedContent page)
        {
            if (page is not CardOverview cardOverview) return new OverviewDataSourceConfig();

            var filters = new List<FilterViewModel>();

            foreach (var filter in cardOverview.Filters.ToItems<OverviewFilter>())
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

            var sortings = cardOverview.Sortings.ToItems<SortingItem>().Select(it => new SortByViewModel(it.DisplayName!, it.Value!)).ToArray();
            if (_memberInfoService.GetMemberInfo()?.IsLoggedIn != true)
            {
                sortings = sortings.Where(it => it.Value != "collection").ToArray();
            }

            var restrictions = cardOverview.InternalFilters.ToItems<RestrictionAttributeBased>().ToArray();
            return new CardOverviewDataSourceConfig
            {
                WhiteBackground = true,
                HideSearch = false,
                HideFilters = cardOverview.HideFilters,

                Filters = [.. filters],
                Sortings = sortings,
                AvailableViews = [OverviewViewType.Images, OverviewViewType.Rows],

                PageSize = cardOverview.PageSize == 0 ? null : cardOverview.PageSize,

                AttributesToShow = cardOverview.AttributesToShow?.OfType<CardAttribute>().ToDictionary(it => it.Name, it => it.DisplayName!) ?? new Dictionary<string, string>(),
                IsValid = (card) =>
                    {
                        foreach (var restriction in restrictions)
                        {
                            var conditionMet = restriction.Rules.ToItems<RestrictionAttribute>()
                                .All(it => it.Values!.Any(c => card.GetMultipleCardAttributeValue(it.IsExpansionRestriction ? "Set Name" : it.Attribute!.Name)?.Contains(c) is true));

                            if (restriction.Process && conditionMet) return false;
                            if (!restriction.Process && !conditionMet) return false;
                        }
                        return true;
                    },
            };
        }
    }
}
