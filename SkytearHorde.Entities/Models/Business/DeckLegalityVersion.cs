using NPoco;

namespace SkytearHorde.Entities.Models.Business
{
    public class DeckLegalityVersion
    {
        public int DeckId { get; set; }
        public int VersionId { get; set; }
        public bool Published { get; set; }
        public bool IsLegal { get; set; }

        [Ignore]
        public int[] CardIds { get; set; } = [];
    }
}
