using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("TournamentEntrant")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class TournamentEntrantDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("TournamentEventId")]
        public Guid TournamentEventId { get; set; }

        [Column("PlayerName")]
        public required string PlayerName { get; set; }

        [Column("Placement")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Placement { get; set; }

        [Column("Wins")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Wins { get; set; }

        [Column("Losses")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Losses { get; set; }

        [Column("Draws")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Draws { get; set; }

        [Column("DeckId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? DeckId { get; set; }

        [Column("ExternalId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ExternalId { get; set; }
    }
}
