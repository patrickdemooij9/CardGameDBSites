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
    public class CardCreateVariantsEventHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;

        public CardCreateVariantsEventHandler(IUmbracoContextFactory umbracoContextFactory, IContentService contentService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            foreach (var item in notification.PublishedEntities)
            {
                if (item.ContentType.Alias != CardVariant.ModelTypeAlias) continue;

                if (ctx.UmbracoContext.Content?.GetById(item.Id) is not CardVariant cardVariant) continue;
                if (cardVariant.VariantType is not Variant variantType) continue;

                var currentVariants = cardVariant.Siblings<CardVariant>()?.Where(it => it.Set == cardVariant.Set && it.VariantType != null).Select(it => it.VariantType as Variant).ToArray() ?? [];
                var variants = cardVariant.Root().FirstChild<Data>()?.FirstChild<VariantsContainer>()?.Children<Variant>()?.ToArray() ?? [];

                var automaticVariants = variants.Where(it => it.AutomaticallyCreateOnVariant == variantType);
                foreach (var variant in automaticVariants)
                {
                    if (currentVariants.Contains(variant))
                    {
                        continue;
                    }

                    var newCardVariant = _contentService.Create($"{variant.DisplayName.IfNullOrWhiteSpace(variant.Name)} {cardVariant.Set!.Name}", cardVariant.Parent!.Key, CardVariant.ModelTypeAlias);
                    newCardVariant.SetValue("variantType", Udi.Create(Constants.UdiEntityType.Document, variant.Key));
                    newCardVariant.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, cardVariant.Set!.Key));
                    _contentService.SaveAndPublish(newCardVariant);
                }
            }
        }
    }
}
