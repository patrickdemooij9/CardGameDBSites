using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("TournamentImportQueue")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TournamentImportQueueDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("FormatId")]
        public int FormatId { get; set; }

        [Column("Type")]
        public string Type { get; set; } = string.Empty;

        [Column("Source")]
        public string Source { get; set; } = string.Empty;

        [Column("ExternalId")]
        public string ExternalId { get; set; } = string.Empty;

        /// <summary>Pending, Processing, Completed, Failed</summary>
        [Column("Status")]
        public string Status { get; set; } = TournamentImportQueueStatus.Pending;

        /// <summary>Result message set once the worker has processed the row.</summary>
        [Column("Message")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Message { get; set; }

        /// <summary>JSON list of missing card descriptions when the import failed on missing cards.</summary>
        [Column("MissingCards")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? MissingCards { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("ProcessedAt")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ProcessedAt { get; set; }
    }

    public static class TournamentImportQueueStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}
