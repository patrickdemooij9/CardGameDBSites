using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.AdServer
{
    [TableName("AdServerMetric")]
    [PrimaryKey("Id")]
    public class MetricDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }
        [Column("AdId")]
        public int AdId { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("Impressions")]
        public int Impressions { get; set; }
        [Column("TrackedImpressions")]
        public int TrackedImpressions { get; set; }

        [Column("Clicks")]
        public int Clicks { get; set; }
        [Column("TrackedClicks")]
        public int TrackedClicks { get; set; }
    }
}
