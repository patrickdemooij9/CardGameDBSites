using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class RedditBotCommentMigration : MigrationBase
    {
        public RedditBotCommentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("RedditBotComment"))
            {
                Create.Table<RedditBotCommentDBModel>().Do();
            }
        }
    }
}
