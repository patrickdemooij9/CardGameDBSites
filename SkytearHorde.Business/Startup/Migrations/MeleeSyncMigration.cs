using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class MeleeSyncMigration : MigrationBase
    {
        public MeleeSyncMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("TournamentEvent", "ExternalId"))
            {
                Create.Column("ExternalId").OnTable("TournamentEvent").AsString(255).Nullable().Do();
            }

            if (!ColumnExists("TournamentEntrant", "ExternalId"))
            {
                Create.Column("ExternalId").OnTable("TournamentEntrant").AsString(255).Nullable().Do();
            }

            if (!IndexExists("IX_TournamentEvent_ExternalId"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_TournamentEvent_ExternalId ON TournamentEvent (ExternalId) WHERE ExternalId IS NOT NULL");
            }

            if (!IndexExists("IX_TournamentEntrant_TournamentEventId_ExternalId"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_TournamentEntrant_TournamentEventId_ExternalId ON TournamentEntrant (TournamentEventId, ExternalId) WHERE ExternalId IS NOT NULL");
            }
        }
    }
}
