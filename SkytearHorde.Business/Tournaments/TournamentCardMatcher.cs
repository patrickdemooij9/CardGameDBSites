using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Tournaments
{
    public class TournamentCardMatcher
    {
        private readonly Dictionary<string, Card> _cardsByName;

        public TournamentCardMatcher(IEnumerable<Card> cards, int deckTypeId)
        {
            // A few cards exist twice under one name, and the illegal one must not make a legal deck illegal.
            _cardsByName = cards
                .GroupBy(it => Normalize(it.DisplayName))
                .ToDictionary(group => group.Key, group => group.FirstOrDefault(it => it.IsLegalFor(deckTypeId)) ?? group.First());
        }

        public Card? Find(string name, string? subtitle)
        {
            var fullName = string.IsNullOrWhiteSpace(subtitle) ? name : $"{name}, {subtitle}";
            return _cardsByName.GetValueOrDefault(Normalize(fullName));
        }

        private static string Normalize(string name) =>
            new(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }
}
