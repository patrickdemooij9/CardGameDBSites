using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckCommentMigration : MigrationBase
    {
        public DeckCommentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckComment"))
            {
                Create.Table<DeckCommentDBModel>().Do();
            }
            if (!TableExists("CardComment"))
            {
                Create.Table<CardCommentDBModel>().Do();
            }
        }
    }
}
