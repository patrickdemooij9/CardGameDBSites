using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.EventHandlers
{
    public class CardVariantsEventHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;
        private readonly CardService _cardService;

        public CardVariantsEventHandler(IUmbracoContextFactory umbracoContextFactory,
            IContentService contentService, CardService cardService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
            _cardService = cardService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var item in notification.PublishedEntities)
            {
                if (item.ContentType.Alias != Card.ModelTypeAlias)
                    continue;

                if (ctx.UmbracoContext.Content?.GetById(item.Id) is not Card contentItem)
                    continue;

                var card = _cardService.Get(item.Id);
                if (card is null) continue;

                var variants = contentItem.Root().FirstChild<Data>()?.FirstChild<VariantsContainer>()?.Children<Variant>()?.ToArray() ?? [];
                var validVariants = variants.Where(it => !it.ManuallyAdd && it.Requirements.ToItems<ISquadRequirementConfig>().All(r => r.GetRequirement().IsValid([card]))).ToArray();

                foreach (var set in contentItem.Set?.OfType<Set>() ?? [])
                {
                    if (contentItem.Children<CardVariant>()?.Any(it => it.Set?.Id == set.Id) is true)
                    {
                        continue; // Skip if variants already exist for this set
                    }

                    var baseVariant = _contentService.Create($"Base {set.Name}", contentItem.Key, CardVariant.ModelTypeAlias);
                    baseVariant.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, set.Key));
                    _contentService.SaveAndPublish(baseVariant);

                    foreach (var variant in validVariants)
                    {
                        var cardVariant = _contentService.Create($"{variant.DisplayName.IfNullOrWhiteSpace(variant.Name)} {set.Name}", contentItem.Key, CardVariant.ModelTypeAlias);
                        cardVariant.SetValue("variantType", Udi.Create(Constants.UdiEntityType.Document, variant.Key));
                        cardVariant.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, set.Key));
                        _contentService.SaveAndPublish(cardVariant);
                    }
                }
            }
        }
    }
}
