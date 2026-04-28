using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.DailyGame
{
    [TableName("DailyCardGameSession")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DailyCardGameSessionDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ChallengeId")]
        [ForeignKey(typeof(DailyCardChallengeDBModel))]
        public int ChallengeId { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("MemberId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MemberId { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "InProgress";

        [Column("AttemptsUsed")]
        public int AttemptsUsed { get; set; }

        [Column("MaxAttempts")]
        public int MaxAttempts { get; set; }

        [Column("StartedUtc")]
        public DateTime StartedUtc { get; set; }

        [Column("FinishedUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? FinishedUtc { get; set; }

        [Column("Solved")]
        public bool Solved { get; set; }
    }
}
