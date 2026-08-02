using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    /// <summary>
    /// A persisted meta snapshot for a single (SiteId, FormatId, PeriodId). Holds the denominator
    /// (<see cref="TotalDecks"/>) used to derive usage percentages; the per-card stats live in
    /// <see cref="MetaCardSnapshotDBModel"/>. Recompute is a full replace of the card rows, so there
    /// is one current snapshot per period.
    /// </summary>
    [TableName("MetaSnapshots")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class MetaSnapshotDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("FormatId")]
        public int FormatId { get; set; }

        [Column("PeriodId")]
        public int PeriodId { get; set; }

        [Column("TotalDecks")]
        public int TotalDecks { get; set; }

        [Column("CreateDateUtc")]
        public DateTime CreateDateUtc { get; set; }

        [Column("LastUpdatedUtc")]
        public DateTime LastUpdatedUtc { get; set; }
    }
}
