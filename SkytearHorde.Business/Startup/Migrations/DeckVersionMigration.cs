using NPoco;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckVersionMigration : MigrationBase
    {
        public DeckVersionMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("DeckVersion"))
            {
                Create.Table<DeckVersionDBModel>().Do();
                Create.Column("VersionId").OnTable("DeckCard").AsInt32().ForeignKey("DeckVersion", "Id").Nullable().Do();
                Alter.Table("DeckCard").AlterColumn("DeckId").AsInt32().Nullable().Do();

                var decks = Database.Fetch<OldDeckDBModel>(Sql()
                    .SelectAll()
                    .From<OldDeckDBModel>());

                foreach(var deck in decks)
                {
                    var versionModel = new DeckVersionDBModel
                    {
                        DeckId = deck.Id,
                        Name = deck.Name,
                        Description = deck.Description,
                        Published = true,
                        IsCurrent = true,
                        CreatedDate = deck.CreatedDate
                    };

                    Database.Insert(versionModel);

                    foreach (var card in Database.Fetch<OldDeckCardDBModel>(Sql()
                        .SelectAll()
                        .From<OldDeckCardDBModel>()
                        .Where<OldDeckCardDBModel>(it => it.DeckId == deck.Id)))
                    {
                        Database.Update(new DeckCardDBModel
                        {
                            Id = card.Id,
                            VersionId = versionModel.Id,
                            Amount = card.Amount,
                            CardId = card.CardId
                        });
                    }
                }

                Delete.ForeignKey("FK_DeckCard_Deck_Id").OnTable("DeckCard").Do();
                Delete.Column("DeckId").FromTable("DeckCard").Do();
                Delete.Column("Name").FromTable("Deck").Do();
                Delete.Column("Description").FromTable("Deck").Do();
                Alter.Table("DeckCard").AlterColumn("VersionId").AsInt32().NotNullable().Do();
            }
        }

        [TableName("Deck")]
        [PrimaryKey("Id", AutoIncrement = true)]
        private class OldDeckDBModel
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("Name")]
            public string Name { get; set; }

            [Column("Description")]
            public string Description { get; set; }

            [Column("CreatedDate")]
            public DateTime CreatedDate { get; set; }

            [Column("CreatedBy")]
            public int CreatedBy { get; set; }
        }

        [TableName("DeckCard")]
        [PrimaryKey("Id", AutoIncrement = true)]
        private class OldDeckCardDBModel
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("DeckId")]
            [ForeignKey(typeof(DeckDBModel), Column = "Id")]
            public int DeckId { get; set; }

            [Column("CardId")]
            public int CardId { get; set; }

            [Column("Amount")]
            public int Amount { get; set; }
        }
    }
}
