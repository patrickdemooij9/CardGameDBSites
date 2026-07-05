using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardImportQueueMigration : MigrationBase
    {
        public CardImportQueueMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("CardImportQueue"))
            {
                Create.Table<CardImportQueueDBModel>().Do();
            }
        }
    }
}
