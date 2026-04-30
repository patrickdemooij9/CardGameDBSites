using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SkytearHorde.Entities.Models.Database
{
    [TableName("DeckArchetype")]
    [PrimaryKey("DeckId,ArchetypeId", AutoIncrement = false)]
    public class DeckArchetypeDBModel
    {
        [Column("DeckId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "DeckId")]
        public int DeckId { get; set; }

        [Column("ArchetypeId")]
        public Guid ArchetypeId { get; set; }
    }
}
