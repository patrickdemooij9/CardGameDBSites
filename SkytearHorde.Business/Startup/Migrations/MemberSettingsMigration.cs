using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class MemberSettingsMigration : MigrationBase
    {
        public MemberSettingsMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("MemberSettings"))
            {
                Create.Table<MemberSettingsDBModel>().Do();
            }
        }
    }
}
