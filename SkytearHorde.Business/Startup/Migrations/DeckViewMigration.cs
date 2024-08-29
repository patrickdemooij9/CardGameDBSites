using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckViewMigration : MigrationBase
    {
        public DeckViewMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckView"))
                Create.Table<DeckViewDBModel>().Do();
        }
    }
}
