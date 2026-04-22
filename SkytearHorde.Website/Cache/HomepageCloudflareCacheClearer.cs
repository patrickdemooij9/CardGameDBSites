using SkytearHorde.Business.Config;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Microsoft.Extensions.Options;

namespace SkytearHorde.Cache
{
    public class HomepageCloudflareCacheClearer : INotificationHandler<ContentPublishedNotification>
    {
        private readonly CloudflareCachePurgeService _cloudflareCachePurgeService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardGameSettingsConfig _config;

        public HomepageCloudflareCacheClearer(CloudflareCachePurgeService cloudflareCachePurgeService,
            IUmbracoContextFactory umbracoContextFactory, IOptions<CardGameSettingsConfig> config)
        {
            _cloudflareCachePurgeService = cloudflareCachePurgeService;
            _umbracoContextFactory = umbracoContextFactory;
            _config = config.Value;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            var hasHomepageUpdate = notification.PublishedEntities.Any(x => x.ContentType.Alias == Homepage.ModelTypeAlias);
            if (!hasHomepageUpdate)
            {
                return;
            }

            var urls = new HashSet<string>(_config.CloudflareHomepageUrls ?? [], StringComparer.OrdinalIgnoreCase);
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            foreach (var item in notification.PublishedEntities.Where(x => x.ContentType.Alias == Homepage.ModelTypeAlias))
            {
                if (ctx.UmbracoContext.Content?.GetById(item.Id) is not Homepage homepage)
                {
                    continue;
                }

                var homepageUrl = homepage.Url(mode: UrlMode.Absolute);
                if (!string.IsNullOrWhiteSpace(homepageUrl))
                {
                    urls.Add(homepageUrl);
                }
            }

            _cloudflareCachePurgeService.PurgeUrls(urls);
        }
    }
}
