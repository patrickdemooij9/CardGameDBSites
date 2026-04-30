using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class SetPriceMigration : MigrationBase
    {
        public SetPriceMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("SetPriceRecord"))
            {
                Create.Table<SetPriceRecordDBModel>().Do();
            }
        }
    }
}
