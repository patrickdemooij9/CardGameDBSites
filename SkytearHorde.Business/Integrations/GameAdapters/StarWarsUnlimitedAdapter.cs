using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Integrations.GameAdapters
{
    public class StarWarsUnlimitedAdapter : IGameAdapter
    {
        private readonly CardService _cardService;

        public int SiteId { get; }
        public int? FormatId { get; }

        public StarWarsUnlimitedAdapter(int siteId, CardService cardService, int? formatId = null)
        {
            SiteId = siteId;
            FormatId = formatId;
            _cardService = cardService;
        }

        public object ExtractKeyAttributes(Deck deck)
        {
            var cardIds = deck.Cards.Select(c => c.CardId).ToArray();
            var cards = _cardService.Get(cardIds).ToList();

            var leader = cards.FirstOrDefault(c =>
                c.Attributes.TryGetValue("Type", out var type) &&
                type?.ToString()?.Equals("Leader", StringComparison.OrdinalIgnoreCase) == true);

            var aspects = cards
                .SelectMany(c =>
                {
                    if (c.Attributes.TryGetValue("Aspect", out var aspect) && aspect is not null)
                        return aspect.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return Array.Empty<string>();
                })
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToArray();

            return new
            {
                LeaderName = leader?.DisplayName,
                Aspects = aspects
            };
        }

        public string SuggestArchetype(Deck deck)
        {
            var cardIds = deck.Cards.Select(c => c.CardId).ToArray();
            var cards = _cardService.Get(cardIds).ToList();

            var leader = cards.FirstOrDefault(c =>
                c.Attributes.TryGetValue("Type", out var type) &&
                type?.ToString()?.Equals("Leader", StringComparison.OrdinalIgnoreCase) == true);

            if (leader is not null)
                return leader.DisplayName;

            var aspects = cards
                .SelectMany(c =>
                {
                    if (c.Attributes.TryGetValue("Aspect", out var aspect) && aspect is not null)
                        return aspect.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return Array.Empty<string>();
                })
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToArray();

            return aspects.Length > 0 ? string.Join(" / ", aspects.Take(2)) : "Unknown";
        }
    }
}
