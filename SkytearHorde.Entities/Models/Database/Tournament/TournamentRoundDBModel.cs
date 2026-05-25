using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.Tournament
{
    [TableName("TournamentRounds")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TournamentRoundDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TournamentId")]
        [ForeignKey(typeof(TournamentDBModel))]
        public int TournamentId { get; set; }

        [Column("RoundNumber")]
        public int RoundNumber { get; set; }

        [Column("Type")]
        public short Type { get; set; }
    }
}
