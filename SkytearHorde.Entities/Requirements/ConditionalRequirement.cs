using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Requirements
{
    public class ConditionalRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.ConditionalAlias;

        private readonly ISquadRequirement[] _conditions;
        private readonly ISquadRequirement[] _requirements;

        public ConditionalRequirement(ISquadRequirement[] conditions, ISquadRequirement[] requirements)
        {
            _conditions = conditions;
            _requirements = requirements;
        }

        public bool IsValid(Card[] cards)
        {
            var cardsToCheck = cards.Where(c => _conditions.All(it => it.IsValid(new[] { c }))).ToArray();

            if (cardsToCheck.Length > 0)
                return _requirements.All(it => it.IsValid(cardsToCheck));
            return true;
        }
    }
}
