using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    /// <summary>
    /// Per-card stats within a <see cref="MetaSnapshotDBModel"/>. Only cards that appear in a
    /// tournament deck in the period get a row; a card without a row reads as 0% usage / 0% winrate.
    /// This is a generic per-card store — it deliberately holds no notion of "leader"; the caller
    /// decides which cards to ask about. Raw counts are stored and percentages derived at read time
    /// (usage = DeckCount / snapshot.TotalDecks, winrate = Wins / (Wins + Losses)).
    /// </summary>
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
    }
}
