using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class EqualValueRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.EqualValueAlias;

        private readonly string _abilityToMatch;
        private readonly string[] _values;

        public EqualValueRequirement(string abilityToMatch, string[] values)
        {
            _abilityToMatch = abilityToMatch;
            _values = values;
        }

        public bool IsValid(Card[] cards)
        {
            foreach (var card in cards)
            {
                var cardValues = card.GetMultipleCardAttributeValue(_abilityToMatch);
                if (cardValues is null || !cardValues.Any(v => _values.Contains(v)))
                    return false;
            }
            return true;
        }
    }
}
