using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    internal class VariantReferenceMigration : MigrationBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public VariantReferenceMigration(IMigrationContext context, IUmbracoContextFactory umbracoContextFactory) : base(context)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        protected override void Migrate()
        {
            var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            foreach (var cardPrice in Database.Fetch<CardPriceRecordDBModel>())
            {
                var card = ctx.UmbracoContext.Content.GetById(cardPrice.CardId) ?? throw new NullReferenceException();
                var variant = cardPrice.VariantId is null ? card.FirstChild<CardVariant>(it => it.VariantType is null)
                    : card.FirstChild<CardVariant>(it => cardPrice.VariantId == (it.VariantType as Variant)?.InternalID);
                if (variant is null)
                {
                    continue;
                }

                cardPrice.VariantId = variant.Id;

                Database.Update(cardPrice);
            }

            Alter.Table("CardPriceRecord").AlterColumn("VariantId").AsInt32().NotNullable().Do();

            foreach (var cardCollection in Database.Fetch<CollectionCardDBModel>().GroupBy(it => it.CardId))
            {
                foreach (var cardVariantGroup in cardCollection.GroupBy(it => it.VariantId))
                {
                    var card = ctx.UmbracoContext.Content.GetById(cardCollection.Key) ?? throw new NullReferenceException();
                    var variant = cardVariantGroup.Key is null ? card.FirstChild<CardVariant>(it => it.VariantType is null)
                        : card.FirstChild<CardVariant>(it => cardVariantGroup.Key == (it.VariantType as Variant)?.InternalID);
                    if (variant is null)
                    {
                        continue;
                    }

                    /*foreach (var itemGroup in cardVariantGroup.InGroupsOf(1000))
                    {
                        var sql = Database.SqlContext.Sql("UPDATE CollectionCard SET VariantID = @0 WHERE Id in (@1)", variant.Id, itemGroup.Select(it => it.Id).ToArray());
                        Database.Execute(sql);
                    }*/

                    if (cardVariantGroup.Key is null)
                    {
                        Database.Execute(Database.SqlContext.Sql($"UPDATE CollectionCard SET VariantID = @0 WHERE CardId = @1 AND VariantID is null", variant.Id, cardCollection.Key));
                    }
                    else
                    {
                        Database.Execute(Database.SqlContext.Sql($"UPDATE CollectionCard SET VariantID = @0 WHERE CardId = @1 AND VariantID = @2", variant.Id, cardCollection.Key, cardVariantGroup.Key));
                    }
                }
            }

            Alter.Table("CollectionCard").AlterColumn("VariantId").AsInt32().NotNullable().Do();
        }
    }
}
