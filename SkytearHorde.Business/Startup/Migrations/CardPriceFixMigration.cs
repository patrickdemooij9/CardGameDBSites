using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    internal class CardPriceFixMigration : MigrationBase
    {
        public CardPriceFixMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            var updatedEntries = new List<CardPriceRecordDBModel>();
            foreach (var entryCardGroup in Database.Query<CardPriceRecordDBModel>(
                Sql()
                .SelectAll()
                .From<CardPriceRecordDBModel>()).GroupBy(it => it.CardId))
            {
                foreach (var entryVariantGroup in entryCardGroup.GroupBy(it => it.VariantId))
                {
                    var firstEntry = entryVariantGroup.OrderBy(it => it.DateUtc).First();
                    var entries = new List<CardPriceRecordDBModel>() { firstEntry };
                    foreach (var entry in entryVariantGroup.Skip(1).OrderBy(it => it.DateUtc))
                    {
                        if (firstEntry.MainPrice == entry.MainPrice && firstEntry.LowestPrice == entry.LowestPrice && firstEntry.HighestPrice == entry.HighestPrice)
                        {
                            Database.Delete(entry);
                            continue;
                        }
                        entries.Add(entry);
                        firstEntry = entry;
                    }

                    var latestEntry = entries.OrderByDescending(it => it.DateUtc).First();
                    latestEntry.IsLatest = true;
                    updatedEntries.Add(latestEntry);
                }
            }
            Database.Execute(Database.SqlContext.Sql("UPDATE CardPriceRecord SET IsLatest = 0"));
            foreach (var entry in updatedEntries)
            {
                Database.Update(entry);
            }
        }
    }
}
