using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class ResourceSquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            var conditionRequirements = MainCardConditions?.Select(it => (it.Content as ISquadRequirementConfig)!.GetRequirement()).ToArray() ?? Array.Empty<ISquadRequirement>();

            var mode = ResourceMode.ContainsAny;
            if (!SingleResourceMode)
            {
                mode = MainAbilityMaxSize > 0 ? ResourceMode.Budget : ResourceMode.Subset;
            }
            return new ResourceRequirement(MainAbility.Name, Ability.Name, conditionRequirements, mode, MainAbilityMaxSize);
        }
        public Dictionary<string, object> GetConfig()
        {
            var conditionRequirements = MainCardConditions?.Select(it => it.Content as ISquadRequirementConfig).ToArray() ?? Array.Empty<ISquadRequirementConfig>();

            var mode = ResourceMode.ContainsAny;
            if (!SingleResourceMode)
            {
                mode = MainAbilityMaxSize > 0 ? ResourceMode.Budget : ResourceMode.Subset;
            }

            return new Dictionary<string, object>
            {
                { "mainAbility", MainAbility.Name },
                { "mainAbilityMaxSize", MainAbilityMaxSize },
                { "ability", Ability.Name },
                { "mainCardsCondition", conditionRequirements.Select(it => new CreateSquadRequirement(it)) },
                { "singleResourceMode", SingleResourceMode },
                { "resourceMode", mode.ToString() },
                { "possibleValues", PossibleValues?.ToArray() ?? [] }
            };
        }
    }
}
