namespace SkytearHorde.Entities.Models.Business
{
    public class DeckTypeLegalityResult
    {
        public int TypeId { get; set; }
        public int IllegalCards { get; set; }
        public int BecameLegal { get; set; }
        public int BecameIllegal { get; set; }
        public int IllegalDecks { get; set; }
    }
}
