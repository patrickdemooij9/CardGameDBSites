using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("CollectionCard")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class CollectionCardDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("VariantId")]
        public int? VariantId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }
    }
}
