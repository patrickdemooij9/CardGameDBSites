using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardPriceDeltaMigration : MigrationBase
    {
        public CardPriceDeltaMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("CardPriceRecord", "Delta"))
            {
                Create.Column("Delta").OnTable("CardPriceRecord").AsDouble().Nullable().Do();

                var allRecords = Database.Query<CardPriceRecordDBModel>(
                    Sql()
                    .SelectAll()
                    .From<CardPriceRecordDBModel>())
                    .ToList();

                foreach (var cardGroup in allRecords.GroupBy(it => it.CardId))
                {
                    foreach (var variantGroup in cardGroup.GroupBy(it => it.VariantId))
                    {
                        var ordered = variantGroup.OrderBy(it => it.DateUtc).ToList();
                        for (var i = 0; i < ordered.Count; i++)
                        {
                            var record = ordered[i];
                            record.Delta = i == 0 ? 0.0 : record.MainPrice - ordered[i - 1].MainPrice;
                            Database.Update(record);
                        }
                    }
                }

                Alter.Table("CardPriceRecord").AlterColumn("Delta").AsDouble().NotNullable().Do();
            }
        }
    }
}
