using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Events;

namespace SkytearHorde.Business.Startup.NotificationHandlers
{
    public class FixSitemapDomainNotificationHandler : INotificationAsyncHandler<GenerateSitemapNodeNotification>
    {
        public Task HandleAsync(GenerateSitemapNodeNotification notification, CancellationToken cancellationToken)
        {
            // Remove after https://github.com/patrickdemooij9/SeoToolkit.Umbraco/issues/548
            if (notification.Node.Url.Contains("api."))
            {
                notification.Node.Url = notification.Node.Url.Replace("api.", "");
            }
            return Task.CompletedTask;
        }
    }
}
