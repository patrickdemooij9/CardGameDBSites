using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services.Site;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Controllers
{
    public class CardOverviewController : SurfaceController
    {
        private readonly ISiteService _siteService;

        public CardOverviewController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, ISiteService siteService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _siteService = siteService;
        }

        public IActionResult Search(string search)
        {
            return Redirect($"{_siteService.GetCardOverview().Url()}?search={search}");
        }
    }
}
