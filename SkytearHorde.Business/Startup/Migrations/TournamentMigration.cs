using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class TournamentMigration : MigrationBase
    {
        public TournamentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("Tournaments"))
            {
                Create.Table<TournamentDBModel>().Do();
            }
            if (!TableExists("TournamentEntrants"))
            {
                Create.Table<TournamentEntrantDBModel>().Do();
            }
            if (!TableExists("TournamentRounds"))
            {
                Create.Table<TournamentRoundDBModel>().Do();
            }
            if (!TableExists("TournamentMatches"))
            {
                Create.Table<TournamentMatchDBModel>().Do();
            }
        }
    }
}
