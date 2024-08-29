namespace SkytearHorde.Entities.Models.Business.Repository
{
    public class DeckPagedResult
    {
        public Deck[] Decks { get; set; } = [];
        public int Total { get; set; }
    }
}
