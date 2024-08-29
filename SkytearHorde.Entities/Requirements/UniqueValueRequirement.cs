using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class UniqueValueRequirement : ISquadRequirement
    {
        public string Alias => "UniqueValue";

        private readonly string _abilityToMatch;

        public UniqueValueRequirement(string abilityToMatch)
        {
            _abilityToMatch = abilityToMatch;
        }

        public bool IsValid(Card[] cards)
        {
            var seenValues = new List<string>();
            foreach (var card in cards)
            {
                var values = card.GetMultipleCardAttributeValue(_abilityToMatch);
                if (values is null) continue;

                if (values.Any(seenValues.Contains)) return false;
                seenValues.AddRange(values);
            }
            return true;
        }
    }
}
