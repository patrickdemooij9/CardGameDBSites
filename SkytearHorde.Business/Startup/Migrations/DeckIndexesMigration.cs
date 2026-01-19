using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckIndexesMigration : MigrationBase
    {
        public DeckIndexesMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Database.Execute("CREATE NONCLUSTERED INDEX IX_Deck_Filter_Score\r\nON dbo.Deck (SiteId, DeckType, IsDeleted, Score DESC)\r\nINCLUDE (Id, CreatedDate, CreatedBy, IsLegal);");
        }
    }
}
