using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("MetaCardSnapshots")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class MetaCardSnapshotDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SnapshotId")]
        [ForeignKey(typeof(MetaSnapshotDBModel))]
        public int SnapshotId { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("DeckCount")]
        public int DeckCount { get; set; }

        [Column("Wins")]
        public int Wins { get; set; }

        [Column("Losses")]
        public int Losses { get; set; }

        [Column("Draws")]
        public int Draws { get; set; }

        [Column("EventCount")]
        public int EventCount { get; set; }

        [Column("Top8Count")]
        public int Top8Count { get; set; }

        [Column("FirstPlaceCount")]
        public int FirstPlaceCount { get; set; }
    }
}
