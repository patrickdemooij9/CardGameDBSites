using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.DailyGame
{
    [TableName("DailyCardChallenge")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DailyCardChallengeDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("ChallengeDateUtc")]
        public DateTime ChallengeDateUtc { get; set; }

        [Column("TargetCardId")]
        public int TargetCardId { get; set; }

        [Column("CreatedUtc")]
        public DateTime CreatedUtc { get; set; }
    }
}
