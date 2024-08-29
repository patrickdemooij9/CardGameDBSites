using AdServer.Finder;
using AdServer.Tracking;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Helpers;
using Umbraco.Cms.Web.Common.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class BannerController : UmbracoApiController
    {
        private readonly AdFinder _adFinder;
        private readonly AdTracker _adTracker;
        private readonly ViewRenderHelper _viewRenderHelper;

        public BannerController(AdFinder adFinder, AdTracker adTracker, ViewRenderHelper viewRenderHelper)
        {
            _adFinder = adFinder;
            _adTracker = adTracker;
            _viewRenderHelper = viewRenderHelper;
        }

        public IActionResult DisplayBanner()
        {
            var domain = Request.Host.Value;
            var ad = _adFinder.GetAdToDisplay(domain);
            if (ad is null) return NotFound();

            _adTracker.AddImpression(ad.Id);
            return Ok(_viewRenderHelper.RenderView("~/Views/Partials/components/advertisementBanner.cshtml", ad));
        }
    }
}
