using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("CardPriceRecord")]
    public class CardPriceRecordDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SourceId")]
        public int SourceId { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("VariantId")]
        public int? VariantId { get; set; }

        [Column("MainPrice")]
        public double MainPrice { get; set; }

        [Column("LowestPrice")]
        public double LowestPrice { get; set; }

        [Column("HighestPrice")]
        public double HighestPrice { get; set; }

        [Column("DateUtc")]
        public DateTime DateUtc { get; set; }

        [Column("IsLatest")]
        public bool IsLatest { get; set; }
    }
}
