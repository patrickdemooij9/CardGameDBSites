using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Helpers
{
    public class SortingDeckHelper
    {
        private readonly IEnumerable<DeckCard> _cards;
        private readonly CardService _cardService;

        public SortingDeckHelper(IEnumerable<DeckCard> cards, CardService cardService)
        {
            _cards = cards;
            _cardService = cardService;
        }

        public IEnumerable<DeckCard> Sort(SortOption[] sortOptions)
        {
            if (sortOptions.Length == 0) return _cards;

            IOrderedEnumerable<DeckCard> orderedCards = sortOptions[0].Values?.Count() > 0 ? _cards.OrderBy(GetOrderByAbilityValue(sortOptions[0])) : _cards.OrderBy(GetOrderByAbility(sortOptions[0]));
            foreach (var sortOption in sortOptions.Skip(1))
            {
                if (sortOption.Values?.Count() > 0)
                    orderedCards = orderedCards.ThenBy(GetOrderByAbilityValue(sortOption));
                else
                    orderedCards = orderedCards.ThenBy(GetOrderByAbility(sortOption));
            }
            return orderedCards;
        }

        private Func<DeckCard, bool> GetOrderByAbilityValue(SortOption sortOption)
        {
            return it =>
            {
                var values = _cardService.Get(it.CardId)?.GetMultipleCardAttributeValue(sortOption.Ability!.Name);
                return values?.Any(v => sortOption.Values!.Contains(v)) != true;
            };
        }

        private Func<DeckCard, string?> GetOrderByAbility(SortOption sortOption)
        {
            return it => _cardService.Get(it.CardId)?.GetMultipleCardAttributeValue(sortOption.Ability!.Name)?.FirstOrDefault();
        }
    }
}
