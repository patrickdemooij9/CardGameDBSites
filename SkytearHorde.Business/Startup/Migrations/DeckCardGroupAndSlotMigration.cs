using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckCardGroupAndSlotMigration : MigrationBase
    {
        public DeckCardGroupAndSlotMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("DeckCard", "GroupId"))
            {
                Create.Column("GroupId").OnTable("DeckCard").AsInt32().Nullable().Do();
            }
            if (!ColumnExists("DeckCard", "SlotId"))
            {
                Create.Column("SlotId").OnTable("DeckCard").AsInt32().Nullable().Do();
            }

            foreach(var cardGrouped in Database.Fetch<DeckCardDBModel>(Sql().SelectAll().From<DeckCardDBModel>()).GroupBy(it => it.VersionId))
            {
                var slotId = 0;
                foreach(var card in cardGrouped)
                {
                    card.GroupId = GroupConstants.MainGroupId;
                    card.SlotId = slotId;

                    Database.Update(card);

                    slotId++;
                }
            }

            //Alter.Table("DeckCard").AlterColumn("GroupId").AsInt32().NotNullable().Do();
            //Alter.Table("DeckCard").AlterColumn("SlotId").AsInt32().NotNullable().Do();
        }
    }
}
