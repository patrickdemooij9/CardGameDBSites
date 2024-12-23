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
        private readonly ICardSearchService _cardSearchService;

        public SquadApiController(CardService cardService, ISiteAccessor siteAccessor, ICardSearchService cardSearchService)
        {
            _cardService = cardService;
            _siteAccessor = siteAccessor;
            _cardSearchService = cardSearchService;
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
            foreach (var filter in filters)
            {
                var selectedValues = filter.Items.Where(it => it.IsChecked).ToArray();
                if (selectedValues.Length == 0) continue;

                query.CustomFields[filter.Alias] = selectedValues.Select(it => it.Value).ToArray();
            }
            return _cardSearchService.Search(query, out _).Select(it => it.BaseId).ToArray();
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
