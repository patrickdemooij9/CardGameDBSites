using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("Tournaments")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TournamentDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("FormatId")]
        public int FormatId { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("DateUtc")]
        public DateTime DateUtc { get; set; }

        [Column("Source")]
        public string Source { get; set; }

        [Column("ExternalUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ExternalUrl { get; set; }

        [Column("ExternalId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ExternalId { get; set; }
    }
}
