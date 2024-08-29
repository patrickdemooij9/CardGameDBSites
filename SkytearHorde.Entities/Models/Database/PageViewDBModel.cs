using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [PrimaryKey("Id", AutoIncrement = true)]
    [TableName("PageView")]
    public class PageViewDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Url")]
        public string Url { get; set; }

        [Column("SessionId")]
        public string SessionId { get; set; }

        [Column("VisitedDate")]
        public DateTime VisitedDate { get; set; }
    }
}
