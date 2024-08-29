using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("CollectionSet")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class CollectionSetDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SetId")]
        public int SetId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("Amount")]
        public int Amount { get; set; }
    }
}
