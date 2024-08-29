using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SkytearHorde.Cache
{
    public class CardOverviewCacheClearer : INotificationHandler<ContentPublishedNotification>
    {
        private readonly CardService _cardService;

        public CardOverviewCacheClearer(CardService cardService)
        {
            _cardService = cardService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            _cardService.ClearCache();
        }
    }
}
