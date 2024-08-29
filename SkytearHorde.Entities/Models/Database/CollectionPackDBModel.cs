using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [PrimaryKey("Id", AutoIncrement = true)]
    [TableName("CollectionPack")]
    public class CollectionPackDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SetId")]
        public int SetId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [Column("Content")]
        public string Content { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }
}
