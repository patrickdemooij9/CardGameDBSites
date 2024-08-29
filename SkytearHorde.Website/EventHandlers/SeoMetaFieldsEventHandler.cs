using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.EventHandlers
{
    public class SeoMetaFieldsEventHandler : INotificationHandler<AfterMetaTagsNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SeoMetaFieldsEventHandler(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public void Handle(AfterMetaTagsNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var currentItem = ctx.UmbracoContext.PublishedRequest?.PublishedContent;
            if (currentItem is null) return;

            if (string.IsNullOrWhiteSpace(notification.MetaTags.Title))
                notification.MetaTags.Title = currentItem.Name;
        }
    }
}
