using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Seo
{
    public class DeckMetaTagsEventHandler : INotificationHandler<AfterMetaTagsNotification>
    {
        private readonly IRequestCache _requestCache;

        public DeckMetaTagsEventHandler(IRequestCache requestCache)
        {
            _requestCache = requestCache;
        }

        public void Handle(AfterMetaTagsNotification notification)
        {
            var deck = _requestCache.Get("Deck") as Deck;
            if (deck is null) return;

            notification.MetaTags.Title = $"{deck.Name} | {notification.MetaTags.Title}";
        }
    }
}
