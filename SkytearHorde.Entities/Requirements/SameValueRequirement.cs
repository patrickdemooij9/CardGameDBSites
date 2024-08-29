using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class SameValueRequirement : ISquadRequirement
    {
        private readonly string _abilityToMatch;
        public string Alias => "SameValue";

        public SameValueRequirement(string abilityToMatch)
        {
            _abilityToMatch = abilityToMatch;
        }

        public bool IsValid(Card[] cards)
        {
            IEnumerable<string>? tempBuffer = null;
            foreach (var card in cards)
            {
                var values = card.GetMultipleCardAttributeValue(_abilityToMatch);
                if (values is null) return false;

                if (tempBuffer is null) tempBuffer = values;
                else tempBuffer = tempBuffer.Intersect(values);
            }
            return tempBuffer?.Count() > 0;
        }
    }
}
