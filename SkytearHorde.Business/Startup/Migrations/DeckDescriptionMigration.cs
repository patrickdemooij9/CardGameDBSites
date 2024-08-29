using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckDescriptionMigration : MigrationBase
    {
        public DeckDescriptionMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "Description"))
            {
                Create.Column("Description").OnTable("Deck").AsString().Nullable().Do();
            }
        }
    }
}
