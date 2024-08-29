using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckVersion")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class DeckVersionDBModel
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey(typeof(DeckDBModel), Column = "Id")]
        [Column("DeckId")]
        public int DeckId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Description { get; set; }

        [Column("Published")]
        public bool Published { get; set; }

        [Column("IsCurrent")]
        public bool IsCurrent { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [ResultColumn]
        public List<DeckCardDBModel> DeckCards { get; set; }

        public DeckVersionDBModel()
        {
            DeckCards = new List<DeckCardDBModel>();
        }
    }
}
