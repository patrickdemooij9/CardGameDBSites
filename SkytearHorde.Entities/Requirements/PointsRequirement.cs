using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public class PointsRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.PointsAlias;

        private readonly string _ability;
        private readonly decimal? _min;
        private readonly decimal? _max;

        public PointsRequirement(string ability, decimal? min, decimal? max)
        {
            _ability = ability;
            _min = min;
            _max = max;
        }

        public bool IsValid(Card[] cards)
        {
            decimal totalPoints = 0;

            foreach (var card in cards)
            {
                var pointValue = card.GetMultipleCardAttributeValue(_ability);
                if (pointValue is null || pointValue.Length == 0)
                {
                    // If a card doesn't have the points attribute, treat it as 0
                    continue;
                }

                if (decimal.TryParse(pointValue[0], out var parsedPoints))
                {
                    totalPoints += parsedPoints;
                }
            }

            if (_min != null && totalPoints < _min)
                return false;
            if (_max != null && totalPoints > _max)
                return false;
            return true;
        }
    }
}
