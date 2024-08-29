using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.EventHandlers
{
    public class CardSetConnectEventHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardService _cardService;

        public CardSetConnectEventHandler(IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            CardService cardService)
        {
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var item in notification.PublishedEntities)
            {
                var publishedItem = ctx.UmbracoContext.Content.GetById(item.Id);
                /*if (publishedItem is Card card)
                {
                    HandleCardConnecting(card);
                }*/
                if (publishedItem is CardVariant variant)
                {
                    HandleVariantConnecting(variant);
                }
                else if (publishedItem is Set set)
                {
                    HandleSetConnecting(set);
                }
            }
        }

        private void HandleCardConnecting(Card card)
        {
            if (card.Set is not Set set) return;

            ConnectCard(card, set);
        }

        private void HandleVariantConnecting(CardVariant variant)
        {
            if (variant.Set is not Set set) return;
            if (variant.VariantType != null) return; // Only connect base cards automatically

            var cards = set.Cards?.OfType<CardVariant>().ToList() ?? new List<CardVariant>();
            if (!cards.Any(it => it.Id == variant.Id))
            {
                cards.Add(variant);
            }

            var sortOptions = variant.Root().FirstChild<Settings>()?.FirstChild<SiteSettings>()?.SortOptions.ToItems<SortOption>().ToArray();
            if (sortOptions?.Length > 0)
            {
                var cardsDict = cards.ToDictionary(it => it.Id, it => it);
                var sortingHelper = new SortingHelper(cards.Select(it => _cardService.Get(it.Id)).WhereNotNull());

                var sortedCards = sortingHelper.Sort(sortOptions);
                cards = sortedCards.Select(it => cardsDict[it.VariantId]).ToList();
            }

            if (cards.Select(it => it.Id).Equals(set.Cards?.Select(it => it.Id))) return;

            var contentItem = _contentService.GetById(set.Id)!;
            contentItem.SetValue("cards", string.Join(',', cards.Select(it => Udi.Create(Constants.UdiEntityType.Document, it.Key))));
            _contentService.SaveAndPublish(contentItem);
        }

        private void HandleSetConnecting(Set set)
        {
            foreach (var item in set.Cards ?? Enumerable.Empty<IPublishedContent>())
            {
                /*if (item is Card card)
                {
                    if (card.Set?.Id == set.Id) continue;

                    var contentItem = _contentService.GetById(card.Id)!;
                    contentItem.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, set.Key));
                    _contentService.SaveAndPublish(contentItem);
                }*/
                if (item is CardVariant variant)
                {
                    if (variant.Set?.Id == set.Id) continue;

                    var contentItem = _contentService.GetById(variant.Id)!;
                    contentItem.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, set.Key));
                    _contentService.SaveAndPublish(contentItem);
                }
            }
        }

        private void ConnectCard(Card card, Set set)
        {
            var cards = set.Cards?.ToList() ?? new List<IPublishedContent>();
            if (!cards.Any(it => it.Id == card.Id))
            {
                cards.Add(card);
            }

            if (cards.Select(it => it.Id).Equals(set.Cards?.Select(it => it.Id))) return;

            var contentItem = _contentService.GetById(set.Id)!;
            contentItem.SetValue("cards", string.Join(',', cards.Select(it => Udi.Create(Constants.UdiEntityType.Document, card.Key))));
            _contentService.SaveAndPublish(contentItem);
        }
    }
}
