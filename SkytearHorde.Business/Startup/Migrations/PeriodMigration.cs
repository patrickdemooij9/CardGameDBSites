using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class PeriodMigration : MigrationBase
    {
        public PeriodMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("Periods"))
            {
                Create.Table<PeriodDBModel>().Do();
            }

            if (!ColumnExists("Tournaments", "SiteId"))
            {
                Create.Column("SiteId").OnTable("Tournaments").AsInt32().Nullable().Do();
                Database.Execute("Update Tournaments set SiteId = 1");
                Alter.Table("Tournaments").AlterColumn("SiteId").AsInt32().NotNullable().Do();
            }

            if (!ColumnExists("Tournaments", "PeriodId"))
            {
                Create.Column("PeriodId").OnTable("Tournaments").AsInt32().Nullable().Do();
            }
        }
    }
}
