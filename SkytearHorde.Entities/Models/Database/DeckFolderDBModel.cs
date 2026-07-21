using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckFolder")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckFolderDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Description { get; set; }

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }
    }
}
