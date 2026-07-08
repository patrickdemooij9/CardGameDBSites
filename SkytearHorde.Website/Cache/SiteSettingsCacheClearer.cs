using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SkytearHorde.Cache
{
    public class SiteSettingsCacheClearer : INotificationHandler<ContentPublishedNotification>
    {
        private readonly SettingsService _settingsService;

        public SiteSettingsCacheClearer(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            _settingsService.ClearSiteSettingsCache();
        }
    }
}
