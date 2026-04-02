using SkytearHorde.Entities.Constants;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Requirements
{
    public enum ResourceMode
    {
        ContainsAny,
        Budget,
        Subset
    }

    public class ResourceRequirement : ISquadRequirement
    {
        public string Alias => RequirementConstants.ResourceAlias;

        private readonly string _mainAbilityName;
        private readonly string _abilityName;
        private readonly ISquadRequirement[] _mainCardsConditions;
        private readonly ResourceMode _resourceMode;
        private readonly int _totalBudget;

        public ResourceRequirement(string mainAbilityName, string abilityName, ISquadRequirement[] mainCardsConditions, ResourceMode resourceMode, int totalBudget = 6)
        {
            _mainAbilityName = mainAbilityName;
            _abilityName = abilityName;
            _mainCardsConditions = mainCardsConditions;
            _resourceMode = resourceMode;
            _totalBudget = totalBudget;
        }

        public bool IsValid(Card[] cards)
        {
            var mainCards = cards.Where(it => _mainCardsConditions.All(c => c.IsValid(new[] { it }))).ToArray();
            if (mainCards.Length == 0) return false;

            var resourcePool = mainCards.SelectMany(it => it.GetMultipleCardAttributeValue(_mainAbilityName)!).GroupBy(it => it).ToDictionary(it => it.Key, it => it.ToArray());

            foreach (var card in cards.Except(mainCards))
            {
                var values = card.GetMultipleCardAttributeValue(_abilityName)?.ToArray();
                if (values is null || values.Length == 0)
                    return false;

                switch (_resourceMode)
                {
                    case ResourceMode.ContainsAny:
                        if (!values.Any(resourcePool.ContainsKey))
                            return false;
                        break;

                    case ResourceMode.Budget:
                        var totalMainResources = resourcePool.Values.Sum(v => v.Length);
                        var remainingBudget = _totalBudget - totalMainResources;
                        var cardResourcePool = values.GroupBy(it => it);
                        var newResourceCount = cardResourcePool
                            .Where(g => !resourcePool.ContainsKey(g.Key))
                            .Sum(g => g.Count());
                        if (newResourceCount > remainingBudget)
                            return false;
                        break;

                    default: // Subset
                        var cardResources = values.GroupBy(it => it);
                        foreach (var resource in cardResources)
                        {
                            if (!resourcePool.ContainsKey(resource.Key))
                                return false;
                            if (resourcePool[resource.Key].Length < resource.Count())
                                return false;
                        }
                        break;
                }
            }
            return true;
        }
    }
}
