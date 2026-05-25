using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("TournamentEntrants")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TournamentEntrantDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TournamentId")]
        [ForeignKey(typeof(TournamentDBModel))]
        public int TournamentId { get; set; }

        [Column("PlayerName")]
        public string PlayerName { get; set; }

        [Column("Placement")]
        public int Placement { get; set; }

        [Column("Wins")]
        public int Wins { get; set; }

        [Column("Losses")]
        public int Losses { get; set; }

        [Column("Draws")]
        public int Draws { get; set; }

        [Column("ExternalId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ExternalId { get; set; }

        [Column("Source")]
        public string Source { get; set; }

        [Column("TournamentDeckId")]
        [ForeignKey(typeof(DeckDBModel))]
        public int TournamentDeckId { get; set; }
    }
}
