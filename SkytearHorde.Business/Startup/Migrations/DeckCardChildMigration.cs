using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckCardChildMigration : MigrationBase
    {
        public DeckCardChildMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckCardChild"))
            {
                Create.Table<DeckCardChildDBModel>().Do();
            }
        }
    }
}
