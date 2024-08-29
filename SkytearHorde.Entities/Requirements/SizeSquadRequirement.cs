using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class SizeSquadRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.SizeAlias;

        private readonly int? _min;
        private readonly int? _max;

        public SizeSquadRequirement(int? min, int? max)
        {
            _min = min;
            _max = max;
        }

        public bool IsValid(Card[] cards)
        {
            if (_min != null && cards.Length < _min)
                return false;
            if (_max != null && cards.Length > _max)
                return false;
            return true;
        }
    }
}
