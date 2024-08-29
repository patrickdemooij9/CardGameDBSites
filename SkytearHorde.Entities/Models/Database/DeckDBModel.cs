using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("Deck")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("CreatedBy")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? CreatedBy { get; set; }

        [Column("DeckType")]
        public int DeckType { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; }

        [Column("Score")]
        public int Score { get; set; }

        [Column("DeletedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? DeletedDate { get; set; }
    }
}
