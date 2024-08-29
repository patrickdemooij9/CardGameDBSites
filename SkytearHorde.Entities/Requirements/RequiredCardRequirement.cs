using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class RequiredCardRequirement : ISquadRequirement
    {
        private readonly int _cardId;

        public string Alias => RequirementConstants.RequiredCardAlias;

        public RequiredCardRequirement(int cardId)
        {
            _cardId = cardId;
        }

        public bool IsValid(Card[] cards)
        {
            return cards.Any(it => it.BaseId == _cardId);
        }
    }
}
