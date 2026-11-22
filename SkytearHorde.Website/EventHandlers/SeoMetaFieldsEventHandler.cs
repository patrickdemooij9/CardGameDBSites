using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.EventHandlers
{
    public class SeoMetaFieldsEventHandler : INotificationHandler<AfterMetaTagsNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly DeckService _deckService;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;
        private readonly IRequestCache _requestCache;

        public SeoMetaFieldsEventHandler(
            IUmbracoContextFactory umbracoContextFactory,
            DeckService deckService,
            CardService cardService,
            SettingsService settingsService, 
            IRequestCache requestCache)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _deckService = deckService;
            _cardService = cardService;
            _settingsService = settingsService;
            _requestCache = requestCache;
        }

        public void Handle(AfterMetaTagsNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var currentItem = notification.Content;
            if (currentItem is null) return;

            var siteSettings = _settingsService.GetSiteSettings();
            if (currentItem is CardVariant cardVariant)
            {
                var card = _requestCache.Get(CardPageService.RequestCacheKey) is Entities.Models.Business.Card cachedCard && cachedCard.VariantId == cardVariant.Id
                    ? cachedCard
                    : _cardService.GetVariant(cardVariant.Id);
                if (card is null) return;

                var set = ctx.UmbracoContext.Content?.GetById(card.SetId) as Set;
                var replacements = new Dictionary<string, string>
                {
                    { "{name}", card.DisplayName },
                    { "{setName}", card.SetName },
                    { "{setCode}", set?.SetCode?.ToUpperInvariant() ?? "" },
                    { "{siteName}", siteSettings.SiteName }
                };
                foreach (var (attribute, value) in card.Attributes)
                {
                    // Ability text stores line breaks as a literal "\r" and keywords as |Keyword|.
                    var text = string.Join(", ", value.GetValues()).Replace("\\r", " ").Replace("|", "");
                    replacements.Add($"{{attributes.{attribute}}}", text);
                }

                notification.MetaTags.Title = string.IsNullOrWhiteSpace(siteSettings.CardMetaTitle)
                    ? card.DisplayName
                    : siteSettings.CardMetaTitle.ReplaceMany(replacements);
                notification.MetaTags.MetaDescription = string.IsNullOrWhiteSpace(siteSettings.CardMetaDescription)
                    ? $"Discover all the features about the card: {card.DisplayName}"
                    : siteSettings.CardMetaDescription.ReplaceMany(replacements);
                return;
            }

            if (currentItem is CardFilterCollection && _requestCache.Get(CardFilterLandingService.RequestCacheKey) is Entities.Models.Business.CardFilterLandingItem generatedLanding)
            {
                notification.MetaTags.Title = $"{generatedLanding.Name} cards | {siteSettings.SiteName}";
                notification.MetaTags.MetaDescription = $"Browse all {generatedLanding.CardCount} {generatedLanding.Name} cards in {siteSettings.SiteName}.";
                return;
            }
            if (string.IsNullOrWhiteSpace(notification.MetaTags.Title) && currentItem is CardFilterLanding)
                notification.MetaTags.Title = $"{currentItem.Name} cards | {siteSettings.SiteName}";
            if (string.IsNullOrWhiteSpace(notification.MetaTags.Title))
                notification.MetaTags.Title = $"{currentItem.Name} | {siteSettings.SiteName}";
            if (!string.IsNullOrWhiteSpace(notification.MetaTags.OpenGraphImage))
                notification.MetaTags.OpenGraphImage = notification.MetaTags.OpenGraphImage.Replace("api.", "");

            if (string.IsNullOrWhiteSpace(notification.MetaTags.MetaDescription) && currentItem.ContentType.Alias == "card")
            {
                notification.MetaTags.MetaDescription = $"Discover all the features about the card: {currentItem.Name}";
            }
            if (currentItem.ContentType.Alias == "deckDetail")
            {
                var deckIdString = ctx.UmbracoContext.OriginalRequestUrl.AbsolutePath.Split('/')[^1];
                if (!int.TryParse(deckIdString, out var deckId)) return;

                var deck = _deckService.Get(deckId);
                if (deck is null) return;

                var replacements = new Dictionary<string, string>
                {
                    { "{name}", deck.Name },
                    { "{type}", _settingsService.GetSquadSettings(deck.TypeId).TypeDisplayName! },
                    { "{siteName}", siteSettings.SiteName }
                };
                var mainCards = _deckService.GetMainCards(deck).ToArray();
                for (var i = 0; i < mainCards.Length; i++)
                {
                    replacements.Add($"{{mainCards.{i}.name}}", mainCards[i].DisplayName);
                }

                notification.MetaTags.Title = notification.MetaTags.Title.ReplaceMany(replacements);
                notification.MetaTags.MetaDescription = notification.MetaTags.MetaDescription.ReplaceMany(replacements);
            }
        }
    }
}
