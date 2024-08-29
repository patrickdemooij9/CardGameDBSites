using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Models.Business
{
    public class CachedDeck : IEntity
    {
        public int Id { get; set; }

        public Deck? PublishedDeck { get; set; }
        public Deck? DraftDeck { get; set; }

        public CachedDeck(Deck deck)
        {
            Id = deck.Id;
            if (deck.IsPublished)
            {
                PublishedDeck = deck;
            }
            else
            {
                DraftDeck = deck;
            }
        }
    }
}
