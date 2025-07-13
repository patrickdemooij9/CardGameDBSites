using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Cms.Web.Common.Controllers;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Business.Middleware;

namespace SkytearHorde.Business.Controllers
{
    public class SquadApiController : UmbracoApiController
    {
        private readonly CardService _cardService;
        private readonly ISiteAccessor _siteAccessor;

        public SquadApiController(CardService cardService, ISiteAccessor siteAccessor)
        {
            _cardService = cardService;
            _siteAccessor = siteAccessor;
        }

        [HttpPost]
        public int[] SearchCards([FromBody]CardOverviewPostModel model)
        {
            if (model is null) return Array.Empty<int>();

            var filters = MapFilter(model);
            var filtersSelected = filters.Any(it => it.Items.Any(item => item.IsChecked));
            if (string.IsNullOrWhiteSpace(model.Search) && !filtersSelected)
            {
                return _cardService.GetAll().Select(it => it.BaseId).ToArray();
            }
            var query = new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId()) { Query = model.Search };
            var searchFilters = new List<CardSearchFilter>();
            foreach (var filter in filters)
            {
                var selectedValues = filter.Items.Where(it => it.IsChecked).ToArray();
                if (selectedValues.Length == 0) continue;

                query.FilterClauses.Add(new CardSearchFilterClause
                {
                    Filters = [new CardSearchFilter
                    {
                        Alias = filter.Alias,
                        Values = selectedValues.Select(it => it.Value).ToArray()
                    }],
                    ClauseType = CardSearchFilterClauseType.AND
                });
            }
            return _cardService.Search(query, out _).Select(it => it.BaseId).ToArray();
        }

        private List<FilterViewModel> MapFilter(CardOverviewPostModel model)
        {
            var filters = new List<FilterViewModel>();
            foreach (var filter in model.Filters)
            {
                filters.Add(new FilterViewModel(filter.Key, filter.Key)
                {
                    Items = filter.Value.Select(it => new FilterItemViewModel(it, it)
                    {
                        IsChecked = true
                    }).ToList()
                });
            };
            return filters;
        }
    }
}
