using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardListMigration : MigrationBase
    {
        public CardListMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("CardList"))
            {
                Create.Table<CardListDBModel>().Do();
            }
            if (!TableExists("CardListItem"))
            {
                Create.Table<CardListItemDBModel>().Do();
            }
        }
    }
}
