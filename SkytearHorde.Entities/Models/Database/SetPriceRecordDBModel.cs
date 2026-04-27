using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("SetPriceRecord")]
    public class SetPriceRecordDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SetId")]
        public int SetId { get; set; }

        [Column("TotalPrice")]
        public double TotalPrice { get; set; }

        [Column("DateUtc")]
        public DateTime DateUtc { get; set; }

        [Column("IsLatest")]
        public bool IsLatest { get; set; }

        [Column("Delta")]
        public double Delta { get; set; }
    }
}
