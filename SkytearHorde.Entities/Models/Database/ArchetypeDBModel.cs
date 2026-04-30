using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("Archetype")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class ArchetypeDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("FormatId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? FormatId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Description { get; set; }
    }
}
