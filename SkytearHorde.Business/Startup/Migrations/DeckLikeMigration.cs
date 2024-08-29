using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckLikeMigration : MigrationBase
    {
        public DeckLikeMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckLike"))
            {
                Create.Table<DeckLikeDBModel>().Do();
            }
        }
    }
}
