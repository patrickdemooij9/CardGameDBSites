using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckSourceMigration : MigrationBase
    {
        public DeckSourceMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "Source"))
            {
                Alter.Table("Deck").AddColumn("Source").AsInt32().WithDefaultValue(0).Do();
            }
        }
    }
}
