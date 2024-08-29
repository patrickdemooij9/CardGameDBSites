namespace SkytearHorde.Entities.Models.Business
{
    public class DeckVersion
    {
        public int Id { get; set; }
        public bool IsPublished { get; set; }
        public List<DeckCard> Cards { get; set; }

        public DeckVersion()
        {
            Cards = new List<DeckCard>();
        }
    }
}
