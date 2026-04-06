using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckVersionIndexUpdateMigration : MigrationBase
    {
        public DeckVersionIndexUpdateMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (IndexExists("DeckVersionCurrent"))
            {
                DeleteIndex<DeckVersionDBModel>("DeckVersionCurrent");
            }

            Database.Execute("CREATE NONCLUSTERED INDEX IX_DeckVersion_Current\r\nON [DeckVersion] ([DeckId], [IsCurrent], [Published])\r\nINCLUDE ([Id], [Name], [Description], [CreatedDate])");
        }
    }
}
