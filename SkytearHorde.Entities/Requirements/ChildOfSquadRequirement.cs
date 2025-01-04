using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Requirements
{
    public class ChildOfSquadRequirement : ISquadRequirement
    {
        private readonly Card _parentCard;

        public string Alias => RequirementConstants.ChildOfAlias;

        public ChildOfSquadRequirement(Card parentCard)
        {
            _parentCard = parentCard;
        }

        public bool IsValid(Card[] cards)
        {
            return cards.All(it => _parentCard.AllowedChildren.Contains(it.BaseId));
        }
    }
}
