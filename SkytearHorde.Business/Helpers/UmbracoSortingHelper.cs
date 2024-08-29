using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Business.Helpers
{
    public class UmbracoSortingHelper
    {
        private readonly IEnumerable<Card> _cards;

        public UmbracoSortingHelper(IEnumerable<Card> cards)
        {
            _cards = cards;
        }

        public IEnumerable<Card> Sort(SortOption[] sortOptions)
        {
            if (sortOptions.Length == 0) return _cards;

            IOrderedEnumerable<Card> orderedCards;
            if (sortOptions[0].Values?.Count() > 0)
            {
                orderedCards = _cards.OrderBy(GetOrderByAbilityValue(sortOptions[0]));
            }
            else
            {
                var ability = sortOptions[0].Ability as Entities.Generated.CardAttribute;
                if (ability?.ExamineIsInteger is true)
                {
                    orderedCards = _cards.OrderBy(GetOrderByAbilityNumber(sortOptions[0]));
                }
                else
                {
                    orderedCards = _cards.OrderBy(GetOrderByAbility(sortOptions[0]));
                }
            }

            foreach (var sortOption in sortOptions.Skip(1))
            {
                if (sortOption.Values?.Count() > 0)
                    orderedCards = orderedCards.ThenBy(GetOrderByAbilityValue(sortOption));
                else
                    orderedCards = orderedCards.ThenBy(GetOrderByAbility(sortOption));
            }
            return orderedCards;
        }

        private Func<Card, bool> GetOrderByAbilityValue(SortOption sortOption)
        {
            return it =>
            {
                var values = it.Attributes.ToItems<IAbilityValue>().FirstOrDefault(it => it.Ability?.Name == sortOption.Ability?.Name)?.GetValues();
                return values?.Any(v => sortOption.Values!.Contains(v)) != true;
            };
        }

        private Func<Card, string?> GetOrderByAbility(SortOption sortOption)
        {
            return it => it.Attributes.ToItems<IAbilityValue>().FirstOrDefault(it => it.Ability?.Name == sortOption.Ability?.Name)?.GetAbilityValue();
        }

        private Func<Card, int?> GetOrderByAbilityNumber(SortOption sortOption)
        {
            return it =>
            {
                var value = it.Attributes.ToItems<IAbilityValue>().FirstOrDefault(it => it.Ability?.Name == sortOption.Ability?.Name)?.GetAbilityValue();
                if (int.TryParse(value, out var result))
                    return result;
                return null;
            };
        }
    }
}
