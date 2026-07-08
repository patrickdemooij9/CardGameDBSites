using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("Periods")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class PeriodDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("FormatId")]
        public int FormatId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("StartingDateUtc")]
        public DateTime StartingDateUtc { get; set; }

        [Column("EndDateUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? EndDateUtc { get; set; }
    }
}
