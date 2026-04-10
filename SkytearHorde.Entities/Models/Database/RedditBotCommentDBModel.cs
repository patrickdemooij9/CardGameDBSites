using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("RedditBotComment")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class RedditBotCommentDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("CommentFullName")]
        public string CommentFullName { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("ProcessedAt")]
        public DateTime ProcessedAt { get; set; }
    }
}
