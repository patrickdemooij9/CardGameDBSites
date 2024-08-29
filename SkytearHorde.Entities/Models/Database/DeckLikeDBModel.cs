using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckLike")]
    [PrimaryKey("UserId,DeckId")]
    public class DeckLikeDBModel
    {
        [Column("UserId")]
        [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "UserId,DeckId")]
        public int UserId { get; set; }

        [Column("DeckId")]
        [ForeignKey(typeof(DeckDBModel), Column = "Id")]
        public int DeckId { get; set; }
    }
}
