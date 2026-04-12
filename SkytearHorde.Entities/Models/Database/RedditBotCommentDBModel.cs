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

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("LastProcessedAt")]
        public DateTime LastProcessedAt { get; set; }
    }
}
