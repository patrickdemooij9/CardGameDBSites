using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class ContentCreatorBlogPostMigration : MigrationBase
    {
        public ContentCreatorBlogPostMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("ContentCreatorBlogPost"))
            {
                Create.Table<ContentCreatorBlogPostDBModel>().Do();
            }
        }
    }
}
