using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Models.Business
{
    public class Deck : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public bool IsPublished { get; set; }
        public required int SiteId { get; set; }
        public required int TypeId { get; set; }
        public List<DeckCard> Cards { get; set; }
        public int AmountOfLikes { get; set; }
        public int Score { get; set; }

        public Deck(string name) : this(0, name){}

        public Deck(int id, string name)
        {
            Id = id;
            Name = name;
            Cards = new List<DeckCard>();

            Score = 0;
            AmountOfLikes = 0;
        }

        public double CalculateCollection(CollectionCardItem[] cards)
        {
            var collectionCards = cards.GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);

            var ownedCards = 0;
            foreach (var card in Cards)
            {
                if (!collectionCards.TryGetValue(card.CardId, out var collectionCard))
                {
                    continue;
                }

                ownedCards += Math.Min(card.Amount, collectionCard.Sum(it => it.Amount));
            }
            return ((double)ownedCards / Cards.Sum(it => it.Amount)) * 100;
        }

        public DeckCard? GetDeckCard(int cardId)
        {
            return Cards.FirstOrDefault(it => it.CardId == cardId);
        }
    }
}
