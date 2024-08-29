using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class PageViewTrackingMigration : MigrationBase
    {
        public PageViewTrackingMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("PageView"))
            {
                Create.Table<PageViewDBModel>().Do();
            }
        }
    }
}
