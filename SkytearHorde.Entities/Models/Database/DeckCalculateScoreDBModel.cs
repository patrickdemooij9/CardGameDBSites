using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckCalculateScore")]
    [PrimaryKey("DeckId", AutoIncrement = false)]
    public class DeckCalculateScoreDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = false)]
        [Column("DeckId")]
        public int DeckId { get; set; }

        [Column("NextCalculateDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? NextCalculateDate { get; set; }
    }
}
