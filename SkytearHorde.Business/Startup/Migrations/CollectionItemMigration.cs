using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CollectionItemMigration : MigrationBase
    {
        public CollectionItemMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("CollectionSet"))
            {
                Create.Table<CollectionSetDBModel>().Do();
            }
            if (!TableExists("CollectionCard"))
            {
                Create.Table<CollectionCardDBModel>().Do();
            }
        }
    }
}
