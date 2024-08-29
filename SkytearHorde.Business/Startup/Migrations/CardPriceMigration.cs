using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardPriceMigration : MigrationBase
    {
        public CardPriceMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("CardPriceRecord"))
            {
                Create.Table<CardPriceRecordDBModel>().Do();
            }
        }
    }
}
