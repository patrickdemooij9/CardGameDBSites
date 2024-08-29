using SkytearHorde.Entities.Models.Database.AdServer;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class AdServerMigration : MigrationBase
    {
        public AdServerMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("AdServerMetricRawData"))
            {
                Create.Table<MetricRawDataDBModel>().Do();
            }
            if (!TableExists("AdServerMetric"))
            {
                Create.Table<MetricDBModel>().Do();
            }
        }
    }
}
