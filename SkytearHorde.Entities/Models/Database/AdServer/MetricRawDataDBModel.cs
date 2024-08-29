using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database.AdServer
{
    [TableName("AdServerMetricRawData")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class MetricRawDataDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("AdId")]
        public int AdId { get; set; }

        [Column("Impression")]
        public bool Impression { get; set; }

        [Column("Click")]
        public bool Click { get; set; }

        [Column("Tracked")]
        public bool Tracked { get; set; }

        [Column("InsertedUtc")]
        public DateTime InsertedUtc { get; set; }
    }
}
