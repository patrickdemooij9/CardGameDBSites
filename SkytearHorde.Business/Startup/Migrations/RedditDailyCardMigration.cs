using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class RedditDailyCardMigration : MigrationBase
    {
        public RedditDailyCardMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("RedditDailyCard"))
            {
                Create.Table<RedditDailyCardDBModel>().Do();
            }
        }
    }
}
