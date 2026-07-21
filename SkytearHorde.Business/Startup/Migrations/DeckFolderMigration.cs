using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckFolderMigration : MigrationBase
    {
        public DeckFolderMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckFolder"))
            {
                Create.Table<DeckFolderDBModel>().Do();
            }

            if (!ColumnExists("Deck", "FolderId"))
            {
                Create.Column("FolderId").OnTable("Deck").AsInt32().Nullable().Do();
            }
        }
    }
}
