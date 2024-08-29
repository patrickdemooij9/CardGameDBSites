using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.EventHandlers
{
    public class CardCreateVariantsEventHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CardCreateVariantsEventHandler(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            foreach (var item in notification.PublishedEntities)
            {
                if (item.ContentType.Alias != Card.ModelTypeAlias) continue;
                if (item.HasIdentity) continue;


            }
        }
    }
}
