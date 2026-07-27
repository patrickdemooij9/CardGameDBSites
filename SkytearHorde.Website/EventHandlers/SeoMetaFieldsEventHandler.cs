using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.EventHandlers
{
    public class SeoMetaFieldsEventHandler : INotificationHandler<AfterMetaTagsNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly DeckService _deckService;
        private readonly SettingsService _settingsService;

        public SeoMetaFieldsEventHandler(IUmbracoContextFactory umbracoContextFactory, DeckService deckService, SettingsService settingsService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _deckService = deckService;
            _settingsService = settingsService;
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

            var siteSettings = _settingsService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(notification.MetaTags.Title))
                notification.MetaTags.Title = $"{currentItem.Name} | {siteSettings.SiteName}";
            if (!string.IsNullOrWhiteSpace(notification.MetaTags.OpenGraphImage))
                notification.MetaTags.OpenGraphImage = notification.MetaTags.OpenGraphImage.Replace("api.", "");

            if (string.IsNullOrWhiteSpace(notification.MetaTags.MetaDescription) && currentItem.ContentType.Alias == "card")
            {
                notification.MetaTags.MetaDescription = $"Discover all the features about the card: {currentItem.Name}";
            }
            if (string.IsNullOrWhiteSpace(notification.MetaTags.MetaDescription) && currentItem.ContentType.Alias == "deckDetail")
            {
                var deckIdString = ctx.UmbracoContext.OriginalRequestUrl.AbsolutePath.Split('/')[^1];
                if (!int.TryParse(deckIdString, out var deckId)) return;

                var deck = _deckService.Get(deckId);
                if (deck is null) return;

                notification.MetaTags.Title = $"{deck.Name} | {siteSettings.SiteName}";
                notification.MetaTags.MetaDescription = $"A {siteSettings.SiteName} deck: ${deck.Name}";
            }
        }

        private (string title, string description) HandleCardVariant(CardVariant cardVariant)
        {
            var card = cardVariant.Parent as Card;
            return (card!.Name, $"Discover all the features about the card: {card.Name}");
        }
    }
}
