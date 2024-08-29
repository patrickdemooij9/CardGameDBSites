using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using SkytearHorde.Entities.Requirements;
using Umbraco.Cms.Core.Models.Blocks;

namespace SkytearHorde.Entities.Generated
{
    public partial class ConditionalSquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            var conditionRequirements = Condition?.Select(it => (it.Content as ISquadRequirementConfig)!.GetRequirement()).ToArray() ?? Array.Empty<ISquadRequirement>();
            var resultRequirements = Requirements?.Select(it => (it.Content as ISquadRequirementConfig)!.GetRequirement()).ToArray() ?? Array.Empty<ISquadRequirement>();

            return new ConditionalRequirement(conditionRequirements, resultRequirements);
        }
        public Dictionary<string, object> GetConfig()
        {
            var conditionRequirements = Condition?.Select(it => it.Content as ISquadRequirementConfig).ToArray() ?? Array.Empty<ISquadRequirementConfig>();
            var resultRequirements = Requirements?.Select(it => it.Content as ISquadRequirementConfig).ToArray() ?? Array.Empty<ISquadRequirementConfig>();

            
            return new Dictionary<string, object>
            {
                { "condition", conditionRequirements.Select(it => new CreateSquadRequirement(it)) },
                { "requirements", resultRequirements.Select(it => new CreateSquadRequirement(it)) }
            };
        }
    }
}
