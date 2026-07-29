using SkytearHorde.Entities.Models.Database.Tournament;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class TournamentImportQueueMigration : MigrationBase
    {
        public TournamentImportQueueMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("TournamentImportQueue"))
            {
                Create.Table<TournamentImportQueueDBModel>().Do();
            }
        }
    }
}
