using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.ViewComponents
{
    public class OverviewViewComponent : ViewComponent
    {
        private readonly IEnumerable<IOverviewDataSource> _dataSources;

        public OverviewViewComponent(IEnumerable<IOverviewDataSource> dataSources)
        {
            _dataSources = dataSources;
        }

        public IViewComponentResult Invoke(IPublishedContent page, OverviewDataSourceKey overviewDataSourceKey)
        {
            var dataSource = _dataSources.First(it => it.SourceKey == overviewDataSourceKey);
            var config = dataSource.GetConfig(page);
            var viewModel = new OverviewViewModel(dataSource, config);

            var searchQuery = Request.Query["search"].ToString();
            var sortBy = Request.Query["sortBy"].ToString();
            var pageNumberString = Request.Query["page"].ToString();
            if (!int.TryParse(pageNumberString, out var pageNumber))
            {
                pageNumber = 1;
            };

            foreach (var filter in config.Filters)
            {
                CheckFilter(filter);
            }

            viewModel.Search = searchQuery;
            viewModel.SortBy = sortBy;
            viewModel.PageId = page.Id;
            viewModel.PageNumber = pageNumber;

            return View("/Views/Partials/components/overview.cshtml", viewModel);
        }

        private void CheckFilter(FilterViewModel filter)
        {
            var selectedValue = Request.Query[filter.Alias];
            if (selectedValue.Count == 0) return;

            foreach (var item in filter.Items)
            {
                item.IsChecked = selectedValue.Contains(item.Value);
            }
        }
    }
}
