using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("ContentCreatorBlogPost")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class ContentCreatorBlogPostDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("BlogId")]
        public string BlogId { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("ImageUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? ImageUrl { get; set; }

        [Column("Url")]
        public string Url { get; set; }

        [Column("PublishedDate")]
        public DateTime PublishedDate { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("CreatorId")]
        public int CreatorId { get; set; }
    }
}
