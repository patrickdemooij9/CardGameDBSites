using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckComment")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckCommentDBModel
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("DeckId")]
        [ForeignKey(typeof(DeckDBModel), Column = "Id")]
        public int DeckId { get; set; }

        [Column("ParentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ParentId { get; set; }

        [Column("SiteId")]
        public int SiteId { get; set; }

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("Comment")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Comment { get; set; }
    }
}
