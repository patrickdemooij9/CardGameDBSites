using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("MemberSettings")]
    [PrimaryKey("MemberId", AutoIncrement = false)]
    public class MemberSettingsDBModel
    {
        [Column("MemberId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int MemberId { get; set; }

        [Column("MissingCopiesThreshold")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MissingCopiesThreshold { get; set; }
    }
}
