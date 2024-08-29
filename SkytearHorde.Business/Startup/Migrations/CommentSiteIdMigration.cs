using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CommentSiteIdMigration : MigrationBase
    {
        public CommentSiteIdMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("CardComment", "SiteId"))
            {
                Create.Column("SiteId").OnTable("CardComment").AsInt32().Do();
            }

            if (!ColumnExists("DeckComment", "SiteId"))
            {
                Create.Column("SiteId").OnTable("DeckComment").AsInt32().Do();
            }
        }
    }
}
