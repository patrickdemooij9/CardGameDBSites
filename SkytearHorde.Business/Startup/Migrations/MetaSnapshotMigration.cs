using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class MetaSnapshotMigration : MigrationBase
    {
        public MetaSnapshotMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("MetaSnapshots"))
            {
                Create.Table<MetaSnapshotDBModel>().Do();
            }

            if (!TableExists("MetaCardSnapshots"))
            {
                Create.Table<MetaCardSnapshotDBModel>().Do();
                Database.Execute("CREATE NONCLUSTERED INDEX IX_MetaCardSnapshots_SnapshotId ON dbo.MetaCardSnapshots (SnapshotId);");
            }
        }
    }
}
