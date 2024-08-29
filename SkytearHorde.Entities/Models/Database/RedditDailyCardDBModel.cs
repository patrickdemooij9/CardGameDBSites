using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("RedditDailyCard")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class RedditDailyCardDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("CardId")]
        public int CardId { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }
    }
}
