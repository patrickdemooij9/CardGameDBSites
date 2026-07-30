using SkytearHorde.Entities.Enums;
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
        public required DeckSource Source { get; set; }
        public required int SiteId { get; set; }
        public required int TypeId { get; set; }
        public List<DeckCard> Cards { get; set; }
        public List<DeckCard> Sideboard { get; set; }
        public int AmountOfLikes { get; set; }
        public int Score { get; set; }
        public bool IsLegal { get; set; }
        public int TotalViews { get; set; }
        public int? FolderId { get; set; }

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

        /// <summary>
        /// Returns true when this deck contains exactly the same cards as <paramref name="other"/> — both
        /// the main deck and the sideboard, matching on card id, group, slot and amount (and each card's
        /// children). Order-independent. Metadata such as name, id, score and dates is ignored.
        /// </summary>
        public bool HasSameCards(Deck? other)
        {
            if (other is null) return false;
            return CardListsEqual(Cards, other.Cards) && CardListsEqual(Sideboard, other.Sideboard);
        }

        private static bool CardListsEqual(List<DeckCard>? a, List<DeckCard>? b)
        {
            a ??= [];
            b ??= [];
            if (a.Count != b.Count) return false;

            return Normalize(a).SequenceEqual(Normalize(b));
        }

        // Canonical, order-independent signature of a card list: one row per card with its position,
        // amount and a stable summary of its children, sorted so two equal lists compare equal.
        private static IEnumerable<(int CardId, int GroupId, int SlotId, int Amount, string Children)> Normalize(List<DeckCard> cards)
        {
            return cards
                .Select(c => (
                    c.CardId,
                    c.GroupId,
                    c.SlotId,
                    c.Amount,
                    Children: string.Join(",", (c.Children ?? [])
                        .GroupBy(ch => ch.CardId)
                        .Select(g => $"{g.Key}x{g.Sum(x => x.Amount)}")
                        .OrderBy(s => s, StringComparer.Ordinal))))
                .OrderBy(t => t.CardId)
                .ThenBy(t => t.GroupId)
                .ThenBy(t => t.SlotId)
                .ThenBy(t => t.Amount)
                .ThenBy(t => t.Children, StringComparer.Ordinal);
        }
    }
}
