using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Events;

namespace SkytearHorde.EventHandlers
{
    public class DynamicPagesSitemapNotificationHandler : INotificationAsyncHandler<GenerateSitemapNotification>
    {
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly SettingsService _settingsService;

        public DynamicPagesSitemapNotificationHandler(CardService cardService, CardPageService cardPageService, SettingsService settingsService)
        {
            _cardService = cardService;
            _cardPageService = cardPageService;
            _settingsService = settingsService;
        }

        public Task HandleAsync(GenerateSitemapNotification notification, CancellationToken cancellationToken)
        {
            var siteSettings = _settingsService.GetSiteSettings();
            foreach (var card in _cardService.GetAll(true).Where(it => it.VariantId > 0 && it.VariantTypeId is null))
            {
                notification.Nodes.Add(new SitemapNodeItem(Path.Join(siteSettings.BaseUrl, _cardPageService.GetUrl(card))));
            }
            return Task.CompletedTask;
        }
    }
}
