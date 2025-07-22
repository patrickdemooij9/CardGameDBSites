using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardPriceLatestMigration : MigrationBase
    {
        public CardPriceLatestMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("CardPriceRecord", "IsLatest"))
            {
                Create.Column("IsLatest").OnTable("CardPriceRecord").AsBoolean().Nullable().Do();

                var updatedEntries = new List<CardPriceRecordDBModel>();
                foreach (var entryCardGroup in Database.Query<CardPriceRecordDBModel>(
                    Sql()
                    .SelectAll()
                    .From<CardPriceRecordDBModel>()).GroupBy(it => it.CardId))
                {
                    foreach (var entryVariantGroup in entryCardGroup.GroupBy(it => it.VariantId))
                    {
                        var latestEntry = entryVariantGroup.OrderByDescending(it => it.DateUtc).First();
                        latestEntry.IsLatest = true;
                        updatedEntries.Add(latestEntry);
                    }
                }
                Database.Execute(Database.SqlContext.Sql("UPDATE CardPriceRecord SET IsLatest = 0"));
                foreach (var entry in updatedEntries)
                {
                    Database.Update(entry);
                }
                Alter.Table("CardPriceRecord").AlterColumn("IsLatest").AsBoolean().NotNullable().Do();
            }
        }
    }
}
