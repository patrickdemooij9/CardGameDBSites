using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class InitialDeckMigration : MigrationBase
    {
        public InitialDeckMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("Deck"))
            {
                Create.Table<DeckDBModel>().Do();
            }
            if (!TableExists("DeckCard"))
            {
                Create.Table<DeckCardDBModel>().Do();
            }
        }
    }
}
