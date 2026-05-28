using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("CardListItem")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class CardListItemDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("ListId")]
        [ForeignKey(typeof(CardListDBModel), Column = "Id")]
        public int ListId { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("VariantId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? VariantId { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }

        [Column("AddedDate")]
        public DateTime AddedDate { get; set; }
    }
}
