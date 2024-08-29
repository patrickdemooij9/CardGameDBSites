using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class AnonymousDeckMigration : MigrationBase
    {
        public AnonymousDeckMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Alter.Table("Deck").AlterColumn("CreatedBy").AsInt32().Nullable().Do();
        }
    }
}
