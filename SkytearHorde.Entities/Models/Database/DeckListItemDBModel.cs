using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckListItem")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckListItemDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("ListId")]
        [ForeignKey(typeof(DeckListDBModel), Column = "Id")]
        public int ListId { get; set; }

        [Column("DeckId")]
        [ForeignKey(typeof(DeckDBModel), Column = "Id")]
        public int DeckId { get; set; }
    }
}
