using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SkytearHorde.Entities.Generated;
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
            var currentItem = notification.Content;
            if (currentItem is null) return;

            if (currentItem is CardVariant cardVariant)
            {
                var (title, description) = HandleCardVariant(cardVariant);
                notification.MetaTags.Title = title;
                notification.MetaTags.MetaDescription = description;
                return;
            }

            if (string.IsNullOrWhiteSpace(notification.MetaTags.Title))
                notification.MetaTags.Title = currentItem.Name;

            if (string.IsNullOrWhiteSpace(notification.MetaTags.MetaDescription) && currentItem.ContentType.Alias == "card")
            {
                notification.MetaTags.MetaDescription = $"Discover all the features about the card: {currentItem.Name}";
            }
        }

        private (string title, string description) HandleCardVariant(CardVariant cardVariant)
        {
            var card = cardVariant.Parent as Card;
            return (card!.Name, $"Discover all the features about the card: {card.Name}");
        }
    }
}
