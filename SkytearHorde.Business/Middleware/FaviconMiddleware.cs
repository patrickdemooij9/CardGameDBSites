using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Middleware
{
    public class FaviconMiddleware
    {
        private readonly string[] _favicons = { "android-chrome-192x192.png", "apple-touch-icon.png", "browserconfig.xml", "favicon.ico", "favicon-16x16.png", "favicon-32x32.png", "mstile-150x150.png", "safari-pinned-tab.svg", "site.webmanifest" };

        private readonly RequestDelegate _next;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;

        public FaviconMiddleware(RequestDelegate next, IUmbracoContextFactory umbracoContextFactory, ISiteService siteService)
        {
            _next = next;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
        }

        public async Task Invoke(HttpContext context)
        {
            var filename = context.Request.GetEncodedUrl().Split('/').Last();
            if (!_favicons.Contains(filename, StringComparer.InvariantCultureIgnoreCase))
            {
                await _next.Invoke(context);
                return;
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var faviconFolder = _siteService.GetSettings().FirstChild<SiteSettings>()!.FaviconFolder;

            if (faviconFolder is null)
            {
                await _next.Invoke(context);
                return;
            }

            var cleanedFilename = filename.Split('.').First().Replace('-', ' ');

            var resultImage = faviconFolder.FirstChild(it => it.Name.Equals(cleanedFilename, StringComparison.InvariantCultureIgnoreCase));
            if (resultImage is null)
            {
                await _next.Invoke(context);
                return;
            }

            var oldPath = context.Request.Path;

            context.Request.Path = resultImage.Url(mode: UrlMode.Relative);
            await _next.Invoke(context);
            context.Request.Path = oldPath;
        }
    }
}
