using DeviceDetectorNET.Class.Device;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.DataSources.Overview;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class OverviewController : SurfaceController
    {
        private readonly ICardSearchService _cardSearchService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly IEnumerable<IOverviewDataSource> _overviewDataSources;

        public OverviewController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, ICardSearchService cardSearchService, ISiteAccessor siteAccessor, CardService cardService, IEnumerable<IOverviewDataSource> overviewDataSources) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _cardSearchService = cardSearchService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _overviewDataSources = overviewDataSources;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SearchCards(CardOverviewPostModel model)
        {
            var filters = MapFilter(model);
            var filtersSelected = filters.Any(it => it.Items.Any(item => item.IsChecked));
            if (string.IsNullOrWhiteSpace(model.Search) && !filtersSelected)
            {
                return Json(_cardService.GetAll().Select(it => it.BaseId).ToArray());
            }
            var query = new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId()) { Query = model.Search };
            foreach (var filter in filters)
            {
                var selectedValues = filter.Items.Where(it => it.IsChecked).ToArray();
                if (selectedValues.Length == 0) continue;

                query.CustomFields[filter.Alias] = selectedValues.Select(it => it.Value).ToArray();
            }
            return Json(_cardSearchService.Search(query, out _).Select(it => it.BaseId).ToArray());
        }

        [HttpPost]
        public IActionResult RenderOverview(CardOverviewPostModel model, bool isAjax = false)
        {
            var dataSource = _overviewDataSources.FirstOrDefault(it => it.SourceKey == model.DataSourceKey);
            if (dataSource is null) return NotFound();

            if (isAjax)
            {
                var page = UmbracoContext.Content?.GetById(model.PageId);
                if (page is null) return NotFound();

                var filters = MapFilter(model);

                var config = dataSource.GetConfig(page);
                config.Filters = filters.ToArray();
                return ViewComponent(dataSource.ViewComponentName, new OverviewDataViewModel(config)
                {
                    PageId = model.PageId,
                    PageNumber = model.PageNumber,
                    SearchQuery = model.Search,
                    SortBy = model.SortBy
                });
            }

            var queryString = new QueryString();
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                queryString = queryString.Add("search", model.Search);
            }
            if (!string.IsNullOrWhiteSpace(model.SortBy))
            {
                queryString = queryString.Add("sortBy", model.SortBy);
            }
            foreach (var filter in model.Filters)
            {
                foreach (var value in filter.Value)
                {
                    queryString = queryString.Add(filter.Key, value);
                }
            }

            return RedirectToCurrentUmbracoPage(queryString);
        }

        private List<FilterViewModel> MapFilter(CardOverviewPostModel model)
        {
            var filters = new List<FilterViewModel>();
            foreach (var filter in model.Filters)
            {
                var filterViewModel = new FilterViewModel(filter.Key, filter.Key);
                foreach (var item in filter.Value.SelectMany(it => it.Split(',')))
                {
                    filterViewModel.Items.Add(new FilterItemViewModel(item, item) { IsChecked = true });
                }
                filters.Add(filterViewModel);
            };
            return filters;
        }
    }
}
