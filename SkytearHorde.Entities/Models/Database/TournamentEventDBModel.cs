using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("TournamentEvent")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class TournamentEventDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("Name")]
        public required string Name { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("FormatId")]
        public int FormatId { get; set; }

        [Column("PlayerCount")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? PlayerCount { get; set; }

        [Column("SourceUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? SourceUrl { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
