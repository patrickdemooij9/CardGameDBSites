using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using SkytearHorde.Entities.Models.TTS;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Generated.Card;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardBaseVariantMigration : MigrationBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IContentService _contentService;

        public CardBaseVariantMigration(IMigrationContext context, IUmbracoContextFactory umbracoContextFactory, IPublishedSnapshotAccessor publishedSnapshotAccessor, IContentService contentService) : base(context)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _contentService = contentService;
        }

        protected override void Migrate()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var allCards = ctx.UmbracoContext.Content.GetByContentType(Card.GetModelContentType(_publishedSnapshotAccessor));

            foreach (var cardsGroup in allCards.GroupBy(it => it.Root().Id))
            {
                var variants = ctx.UmbracoContext.Content.GetById(cardsGroup.Key)?
                    .FirstChild<Data>()?
                    .FirstChild<VariantsContainer>()?
                    .Children<Variant>()?
                    .ToArray();

                foreach (var card in cardsGroup.OfType<Card>())
                {
                    CreateOrUpdateVariant(card, null, "Base");

                    if (variants?.Length > 0)
                    {
                        foreach (var variant in variants)
                        {
                            var requirements = variant.Requirements.ToItems<ISquadRequirementConfig>().ToArray();
                            if (variant.RequiresPage && requirements.Length == 0)
                            {
                                //CleanupVariant(card, variant);
                                continue;
                            }

                            var businessCard = Entities.Models.Business.Card.Map(card);
                            if (requirements.Any(it => !it.GetRequirement().IsValid([businessCard]))) continue;

                            CreateOrUpdateVariant(card, variant, variant.Name);
                        }
                    }
                }
            }
        }

        private void CreateOrUpdateVariant(Card card, Variant? variantType, string name)
        {
            var baseVariant = card.FirstChild<CardVariant>(it => it.VariantType?.Key == variantType?.Key);
            var newBase = baseVariant is null ? _contentService.CreateAndSave(name, card.Id, CardVariant.ModelTypeAlias) : _contentService.GetById(baseVariant.Id)!;

            foreach (var oldBase in card.Children<CardVariant>()?.Where(it => it.VariantType?.Key == variantType?.Key && it != baseVariant) ?? [])
            {
                _contentService.Delete(_contentService.GetById(oldBase.Key)!);
            }
            if (variantType != null)
            {
                newBase.SetValue("variantType", Udi.Create(Constants.UdiEntityType.Document, variantType.Key));
            }

            newBase.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, card.Set?.Key ?? card.Parent<Set>()?.Key ?? throw new NotImplementedException("Needs either a set or a parent of set")));

            _contentService.SaveAndPublish(newBase);
        }

        private void CleanupVariant(Card card, Variant? variantType)
        {
            foreach (var oldBase in card.Children<CardVariant>()?.Where(it => it.VariantType?.Key == variantType?.Key) ?? [])
            {
                _contentService.Delete(_contentService.GetById(oldBase.Key)!);
            }
        }
    }
}
