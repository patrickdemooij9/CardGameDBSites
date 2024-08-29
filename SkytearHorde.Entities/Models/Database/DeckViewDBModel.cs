using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckView")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckViewDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("DeckId")]
        [ForeignKey(typeof(DeckDBModel))]
        public int DeckId { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("Views")]
        public int Views { get; set; }
    }
}
