using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    // Requirement where cards gives resources that are then required by the other ones.
    // Almost the same as SameValueRequirement, but not all cards need to have the same requirements as each other.
    public class ResourceRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.ResourceAlias;

        private readonly string _mainAbilityName;
        private readonly string _abilityName;
        private readonly ISquadRequirement[] _mainCardsConditions;
        private readonly bool _singleResourceMode;

        public ResourceRequirement(string mainAbilityName, string abilityName, ISquadRequirement[] mainCardsConditions, bool singleResourceMode)
        {
            _mainAbilityName = mainAbilityName;
            _abilityName = abilityName;
            _mainCardsConditions = mainCardsConditions;
            _singleResourceMode = singleResourceMode;
        }

        public bool IsValid(Card[] cards)
        {
            var mainCards = cards.Where(it => _mainCardsConditions.All(c => c.IsValid(new[] { it }))).ToArray();
            if (mainCards.Length == 0) return false;

            var resourcePool = mainCards.SelectMany(it => it.GetMultipleCardAttributeValue(_mainAbilityName)!).GroupBy(it => it).ToDictionary(it => it.Key, it => it.ToArray());
            foreach(var card in cards.Except(mainCards))
            {
                var values = card.GetMultipleCardAttributeValue(_abilityName)?.ToArray();
                if (values is null || values.Length == 0)
                    return false;

                var cardResourcePool = values.GroupBy(it => it);
                foreach (var resource in cardResourcePool)
                {
                    if (!resourcePool.ContainsKey(resource.Key))
                        return false;
                    if (!_singleResourceMode)
                        continue;
                    if (resourcePool[resource.Key].Length >= resource.Count())
                        continue;
                    return false;
                }
            }
            return true;
        }
    }
}
