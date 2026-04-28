using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.DailyGame
{
    [TableName("DailyCardGameGuess")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DailyCardGameGuessDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SessionId")]
        [ForeignKey(typeof(DailyCardGameSessionDBModel))]
        public int SessionId { get; set; }

        [Column("AttemptNumber")]
        public int AttemptNumber { get; set; }

        [Column("GuessedCardId")]
        public int GuessedCardId { get; set; }

        [Column("FeedbackJson")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string FeedbackJson { get; set; } = string.Empty;

        [Column("CreatedUtc")]
        public DateTime CreatedUtc { get; set; }
    }
}
