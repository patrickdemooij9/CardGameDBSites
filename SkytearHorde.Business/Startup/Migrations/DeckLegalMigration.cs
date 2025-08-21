using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckLegalMigration : MigrationBase
    {
        public DeckLegalMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "IsLegal"))
            {
                Create.Column("IsLegal").OnTable("Deck")
                    .AsBoolean()
                    .Nullable()
                    .Do();

                Database.Execute("UPDATE Deck SET IsLegal = 1 WHERE IsLegal IS NULL;");

                Alter.Table("Deck")
                    .AlterColumn("IsLegal")
                    .AsBoolean()
                    .NotNullable()
                    .Do();
            }
        }
    }
}
