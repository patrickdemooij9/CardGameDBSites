using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Requirements
{
    public class ComputedRequirement : ISquadRequirement
    {
        private readonly ComputedRequirementConfig _config;

        public string Alias => RequirementConstants.ComputedAlias;

        public ComputedRequirement(ComputedRequirementConfig config)
        {
            _config = config;
        }

        public bool IsValid(Card[] cards)
        {
            var matchingFirstAbility = cards.Where(it => _config.FirstAbilityRequirement.IsValid([it])).ToArray();
            var matchingSecondAbility = cards.Where(it => _config.SecondAbilityRequirement.IsValid([it])).ToArray();

            var firstAbilityComputed = matchingFirstAbility.Select(it =>
            {
                var value = it.GetMultipleCardAttributeValue(_config.FirstAbilityValue);
                if (value is null || !int.TryParse(value[0], out var parsedValue)) return 0;
                return parsedValue;
            });
            var secondAbilityComputed = matchingFirstAbility.Select(it =>
            {
                var value = it.GetMultipleCardAttributeValue(_config.SecondAbilityValue);
                if (value is null || !int.TryParse(value[0], out var parsedValue)) return 0;
                return parsedValue;
            });

            var firstValue = GetComputedValue(firstAbilityComputed, _config.FirstAbilityType);
            var secondValue = GetComputedValue(secondAbilityComputed, _config.SecondAbilityType);
            return CompareValues(firstValue, secondValue, _config.Comparison);
        }

        private int GetComputedValue(IEnumerable<int> values, ComputedType computedType)
        {
            switch (computedType)
            {
                case ComputedType.Sum:
                    return values.Sum();
                case ComputedType.Count:
                    return values.Count();
            }
            return 0;
        }

        private bool CompareValues(int firstValue, int secondValue, ComputedComparisonType comparisonType)
        {
            switch (comparisonType)
            {
                case ComputedComparisonType.Equal:
                    return firstValue == secondValue;
                case ComputedComparisonType.HigherThan:
                    return firstValue >= secondValue;
                case ComputedComparisonType.LowerThan:
                    return firstValue <= secondValue;
            }
            return false;
        }
    }
}
