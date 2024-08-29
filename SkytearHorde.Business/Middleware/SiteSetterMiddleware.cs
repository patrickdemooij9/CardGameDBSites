using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Middleware
{
    public class SiteSetterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IRuntimeState _runtimeState;

        public SiteSetterMiddleware(RequestDelegate next, IUmbracoContextFactory umbracoContextFactory, IRuntimeState runtimeState)
        {
            _next = next;
            _umbracoContextFactory = umbracoContextFactory;
            _runtimeState = runtimeState;
        }

        public async Task Invoke(HttpContext context, ISiteAccessor siteAccessor)
        {
            SetSiteId(context, siteAccessor);

            await _next.Invoke(context);
        }

        private void SetSiteId(HttpContext context, ISiteAccessor siteAccessor)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();
                var currentDomain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                if (currentDomain is null) return;

                var siteId = ctx.UmbracoContext.Content?.GetById(currentDomain.ContentId)?.FirstChild<Settings>()?.FirstChild<SiteSettings>()?.SiteId;
                if (siteId is null) return;

                siteAccessor.SetSiteId(siteId.Value);
            }
        }
    }
}
