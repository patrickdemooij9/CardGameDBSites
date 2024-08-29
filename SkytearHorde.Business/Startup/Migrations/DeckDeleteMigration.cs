using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckDeleteMigration : MigrationBase
    {
        public DeckDeleteMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "IsDeleted"))
            {
                Create.Column("IsDeleted").OnTable("Deck").AsBoolean().WithDefaultValue(false).Do();
            }
            if (!ColumnExists("Deck", "DeletedDate"))
            {
                Create.Column("DeletedDate").OnTable("Deck").AsDateTime().Nullable().Do();
            }
        }
    }
}
