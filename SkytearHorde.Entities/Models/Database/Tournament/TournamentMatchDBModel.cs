using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("TournamentMatches")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TournamentMatchDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("RoundId")]
        [ForeignKey(typeof(TournamentRoundDBModel))]
        public int RoundId { get; set; }

        [Column("Entrant1Id")]
        [ForeignKey(typeof(TournamentEntrantDBModel), Name = "FK_TournamentMatches_TournamentEntrants_Entrant1Id")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Entrant1Id { get; set; }

        [Column("Entrant2Id")]
        [ForeignKey(typeof(TournamentEntrantDBModel), Name = "FK_TournamentMatches_TournamentEntrants_Entrant2Id")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Entrant2Id { get; set; }

        [Column("WinnerEntrantId")]
        [ForeignKey(typeof(TournamentEntrantDBModel))]
        public int WinnerEntrantId { get; set; }

        [Column("GamesWonP1")]
        public int GamesWonP1 { get; set; }

        [Column("GamesWonP2")]
        public int GamesWonP2 { get; set; }
    }
}
