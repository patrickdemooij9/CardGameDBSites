using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckTypeMigration : MigrationBase
    {
        public DeckTypeMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "DeckType"))
            {
                Create.Column("DeckType").OnTable("Deck").AsInt32().Nullable().Do();
                Database.Execute("update Deck set DeckType = 1");
                Alter.Table("Deck").AlterColumn("DeckType").AsInt32().NotNullable().Do();
            }
        }
    }
}
