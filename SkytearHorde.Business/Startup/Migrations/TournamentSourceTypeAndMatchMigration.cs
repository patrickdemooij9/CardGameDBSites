using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class TournamentSourceTypeAndMatchMigration : MigrationBase
    {
        public TournamentSourceTypeAndMatchMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("TournamentEvent", "SourceType"))
            {
                Create.Column("SourceType").OnTable("TournamentEvent").AsString(128).Nullable().Do();
            }

            if (!TableExists("TournamentMatch"))
            {
                Create.Table<TournamentMatchDBModel>().Do();
            }

            if (!IndexExists("IX_TournamentMatch_TournamentEventId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_TournamentMatch_TournamentEventId ON TournamentMatch (TournamentEventId)");
            }

            if (!IndexExists("IX_TournamentMatch_TournamentEntrantId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_TournamentMatch_TournamentEntrantId ON TournamentMatch (TournamentEntrantId)");
            }
        }
    }
}
