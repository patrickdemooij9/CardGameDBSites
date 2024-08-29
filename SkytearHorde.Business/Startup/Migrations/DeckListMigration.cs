using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckListMigration : MigrationBase
    {
        public DeckListMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckList"))
            {
                Create.Table<DeckListDBModel>().Do();
            }
            if (!TableExists("DeckListItem"))
            {
                Create.Table<DeckListItemDBModel>().Do();
            }
        }
    }
}
