using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
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

        /// <summary>
        /// Start (Monday 00:00 UTC) of the ISO week this snapshot covers. The snapshot includes every
        /// tournament in the period dated before <c>SnapshotDateUtc + 7 days</c>.
        /// </summary>
        [Column("SnapshotDateUtc")]
        public DateTime SnapshotDateUtc { get; set; }

        /// <summary>The most recent week's snapshot for this (SiteId, FormatId, PeriodId). Exactly one per period.</summary>
        [Column("IsLatest")]
        public bool IsLatest { get; set; }

        [Column("TotalDecks")]
        public int TotalDecks { get; set; }

        [Column("CreateDateUtc")]
        public DateTime CreateDateUtc { get; set; }

        [Column("LastUpdatedUtc")]
        public DateTime LastUpdatedUtc { get; set; }
    }
}
