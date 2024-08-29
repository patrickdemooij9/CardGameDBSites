using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckCalculateMigration : MigrationBase
    {
        public DeckCalculateMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("DeckCalculateScore")) return;

            Create.Table<DeckCalculateScoreDBModel>().Do();
            Alter.Table("Deck").AddColumn("Score").AsInt32().Nullable().Do();
            foreach (var deck in Database.Fetch<DeckDBModel>())
            {
                deck.Score = 0;
                Database.Update(deck);
                Database.Insert(new DeckCalculateScoreDBModel
                {
                    DeckId = deck.Id,
                    NextCalculateDate = DateTime.UtcNow.AddMinutes(1)
                });
            }
            Alter.Table("Deck").AlterColumn("Score").AsInt32().NotNullable().Do();
        }
    }
}
