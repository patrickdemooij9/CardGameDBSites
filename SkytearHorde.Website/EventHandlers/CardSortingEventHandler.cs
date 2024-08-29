using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.EventHandlers
{
    public class CardSortingEventHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;
        private readonly CardService _cardService;

        public CardSortingEventHandler(IUmbracoContextFactory umbracoContextFactory, IContentService contentService,
            CardService cardService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
            _cardService = cardService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            foreach (var content in notification.PublishedEntities)
            {
                if (content.ContentType.Alias == SiteSettings.ModelTypeAlias)
                {
                    if (!content.WasPropertyDirty("sortOptions")) continue;
                    AfterSiteSettingsUpdated(content.Id);
                }
                else if (content.ContentType.Alias == Card.ModelTypeAlias)
                {
                    AfterCardsUpdated(content.Id);
                }
            }
        }

        private void AfterSiteSettingsUpdated(int id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var settings = ctx.UmbracoContext.Content!.GetById(id) as SiteSettings;

            var sortOptions = settings!.SortOptions.ToItems<SortOption>().ToArray();
            if (sortOptions.Length == 0) return;

            var setContainer = settings?.Root().FirstChild<Data>()?.FirstChild<SetContainer>();
            if (setContainer is null) return;

            SortCards(sortOptions);
        }

        private void AfterCardsUpdated(int id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var card = ctx.UmbracoContext.Content!.GetById(id) as Card;

            if (card is null) return;

            var settings = card.Root().FirstChild<Settings>()?.FirstChild<SiteSettings>();
            if (settings is null) return;

            var sortOptions = settings!.SortOptions.ToItems<SortOption>().ToArray();
            if (sortOptions.Length == 0) return;

            SortCards(sortOptions);
        }

        private void SortCards(SortOption[] sortOptions)
        {
            var cards = _cardService.GetAll().ToArray();
            var sets = _cardService.GetAllSets().ToArray();

            var sortedCards = new List<int>();
            foreach (var set in sets)
            {
                var sortingHelper = new SortingHelper(cards.Where(it => it.SetId == set.Id));
                sortedCards.AddRange(sortingHelper.Sort(sortOptions).Select(x => x.BaseId));
            }

            _contentService.Sort(sortedCards);
        }
    }
}
