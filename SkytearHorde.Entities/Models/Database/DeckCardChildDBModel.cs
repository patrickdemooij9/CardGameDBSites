using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckCardChild")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckCardChildDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("VersionId")]
        [ForeignKey(typeof(DeckVersionDBModel), Column = "Id")]
        public int VersionId { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }

        [Column("ParentId")]
        [ForeignKey(typeof(DeckCardDBModel), Column = "Id")]
        public int ParentId { get; set; }
    }
}
