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

            return new ResourceRequirement(MainAbility.Name, Ability.Name, conditionRequirements, RequireAllResources);
        }
        public Dictionary<string, object> GetConfig()
        {
            var conditionRequirements = MainCardConditions?.Select(it => it.Content as ISquadRequirementConfig).ToArray() ?? Array.Empty<ISquadRequirementConfig>();

            return new Dictionary<string, object>
            {
                { "mainAbility", MainAbility.Name },
                { "ability", Ability.Name },
                { "mainCardsCondition", conditionRequirements.Select(it => new CreateSquadRequirement(it)) },
                { "requireAllResources", RequireAllResources }
            };
        }
    }
}
