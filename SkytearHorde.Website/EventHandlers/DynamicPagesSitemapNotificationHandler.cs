using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.EventHandlers
{
    public class DynamicPagesSitemapNotificationHandler : INotificationAsyncHandler<GenerateSitemapNotification>
    {
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly MetaCardPageService _metaCardPageService;
        private readonly SettingsService _settingsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly CardFilterLandingService _cardFilterLandingService;

        public DynamicPagesSitemapNotificationHandler(CardService cardService, CardPageService cardPageService, MetaCardPageService metaCardPageService, SettingsService settingsService, IUmbracoContextFactory umbracoContextFactory, ISiteService siteService, CardFilterLandingService cardFilterLandingService)
        {
            _cardService = cardService;
            _cardPageService = cardPageService;
            _metaCardPageService = metaCardPageService;
            _settingsService = settingsService;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _cardFilterLandingService = cardFilterLandingService;
        }

        public Task HandleAsync(GenerateSitemapNotification notification, CancellationToken cancellationToken)
        {
            var siteSettings = _settingsService.GetSiteSettings();
            foreach (var card in _cardService.GetAll(true).Where(it => it.VariantId > 0 && it.VariantTypeId is null))
            {
                notification.Nodes.Add(new SitemapNodeItem(Path.Join(siteSettings.BaseUrl, _cardPageService.GetUrl(card))));
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var collection in _cardFilterLandingService.GetCollections())
            {
                notification.Nodes.Add(new SitemapNodeItem(siteSettings.BaseUrl + collection.Url));
                foreach (var landing in collection.Landings)
                {
                    notification.Nodes.Add(new SitemapNodeItem(siteSettings.BaseUrl + landing.Url));
                }
            }

            var rootNode = _siteService.GetRoot();
            foreach (var node in rootNode.Children<MetaPage>() ?? [])
            {
                notification.Nodes.Add(new SitemapNodeItem(siteSettings.BaseUrl + '/' + node.UrlSegment));

                foreach (var childNode in node.Descendants())
                {
                    if (childNode is MetaCardDetail)
                    {
                        foreach (var card in _metaCardPageService.GetCardsForOverview())
                        {
                            var cardUrl = _metaCardPageService.GetMetaUrlForCard(card);
                            if (string.IsNullOrWhiteSpace(cardUrl))
                            {
                                continue;
                            }

                            notification.Nodes.Add(new SitemapNodeItem(siteSettings.BaseUrl + cardUrl));
                        }
                        continue;
                    }

                    notification.Nodes.Add(new SitemapNodeItem(siteSettings.BaseUrl + childNode.Url(mode: UrlMode.Relative)));
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
