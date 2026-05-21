using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("TournamentMatch")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class TournamentMatchDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("TournamentEventId")]
        public Guid TournamentEventId { get; set; }

        [Column("TournamentEntrantId")]
        public Guid TournamentEntrantId { get; set; }

        [Column("RoundNumber")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? RoundNumber { get; set; }

        [Column("OpponentName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? OpponentName { get; set; }

        [Column("Wins")]
        public int Wins { get; set; }

        [Column("Losses")]
        public int Losses { get; set; }

        [Column("Draws")]
        public int Draws { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("ExternalId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ExternalId { get; set; }
    }
}
