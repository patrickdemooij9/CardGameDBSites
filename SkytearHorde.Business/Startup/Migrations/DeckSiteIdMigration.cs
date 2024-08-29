using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckSiteIdMigration : MigrationBase
    {
        public DeckSiteIdMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "SiteId"))
            {
                Create.Column("SiteId").OnTable("Deck").AsInt32().Nullable().Do();

                Database.Execute("Update Deck set SiteId = 1");

                Alter.Table("Deck").AlterColumn("SiteId").AsInt32().NotNullable().Do();
            }
        }
    }
}
