using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardImportQueueSetMigration : MigrationBase
    {
        public CardImportQueueSetMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("CardImportQueue", "SetId"))
            {
                Create.Column("SetId").OnTable("CardImportQueue").AsInt32().Nullable().Do();
            }
        }
    }
}
