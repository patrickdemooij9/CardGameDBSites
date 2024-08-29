using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckCard")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckCardDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("VersionId")]
        [ForeignKey(typeof(DeckVersionDBModel), Column = "Id")]
        public int VersionId { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("GroupId")]
        public int GroupId { get; set; }

        [Column("SlotId")]
        public int SlotId { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }
    }
}
