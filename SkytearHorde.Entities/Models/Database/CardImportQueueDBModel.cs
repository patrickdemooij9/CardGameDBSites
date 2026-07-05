using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("CardImportQueue")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class CardImportQueueDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        /// <summary>JSON object containing all AI-extracted card properties (Name, Cost, Aspects, etc.)</summary>
        [Column("ExtractedData")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string ExtractedData { get; set; } = string.Empty;

        /// <summary>Pending, Approved, Rejected, PotentialVariant</summary>
        [Column("Status")]
        public string Status { get; set; } = CardImportQueueStatus.Pending;

        /// <summary>Discord, Reddit, Manual</summary>
        [Column("SourceType")]
        public string SourceType { get; set; } = string.Empty;

        [Column("SourceUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? SourceUrl { get; set; }

        /// <summary>Path to the saved card image file on disk</summary>
        [Column("ImagePath")]
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>Path to the back image file (Leaders only)</summary>
        [Column("BackImagePath")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? BackImagePath { get; set; }

        /// <summary>Perceptual hash (dHash) for near-duplicate detection</summary>
        [Column("ImageHash")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ImageHash { get; set; }

        /// <summary>Id of an existing card with the same name — set when Status is PotentialVariant</summary>
        [Column("PotentialDuplicateId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? PotentialDuplicateId { get; set; }

        /// <summary>Id of the set this card is linked to — required before the item can be approved</summary>
        [Column("SetId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? SetId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("ReviewedAt")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ReviewedAt { get; set; }
    }

    public static class CardImportQueueStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string PotentialVariant = "PotentialVariant";
    }

    public static class CardImportQueueSource
    {
        public const string Discord = "Discord";
        public const string Reddit = "Reddit";
        public const string Manual = "Manual";
    }
}
