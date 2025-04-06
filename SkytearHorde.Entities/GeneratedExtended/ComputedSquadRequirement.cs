using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class ComputedSquadRequirement
    {
        public ISquadRequirement GetRequirement()
        {
            var firstRequirement = FirstAbilityRequirement?.Select(it => (it.Content as ISquadRequirementConfig)!.GetRequirement()).FirstOrDefault();
            var secondRequirement = SecondAbility?.Select(it => (it.Content as ISquadRequirementConfig)!.GetRequirement()).FirstOrDefault();

            return new ComputedRequirement(new Models.Business.ComputedRequirementConfig
            {
                FirstAbilityRequirement = firstRequirement,
                FirstAbilityType = (ComputedType)Enum.Parse(typeof(ComputedType), FirstAbilityCompute),
                FirstAbilityValue = FirstAbilityValue,
                Comparison = (ComputedComparisonType)Enum.Parse(typeof(ComputedComparisonType), Comparison),
                SecondAbilityRequirement = secondRequirement,
                SecondAbilityType = (ComputedType)Enum.Parse(typeof(ComputedType), SecondAbilityCompute),
                SecondAbilityValue = SecondAbilityValue,
            });
        }
        public Dictionary<string, object> GetConfig()
        {
            var firstRequirement = FirstAbilityRequirement?.Select(it => (it.Content as ISquadRequirementConfig)).FirstOrDefault();
            var secondRequirement = SecondAbility?.Select(it => (it.Content as ISquadRequirementConfig)).FirstOrDefault();

            return new Dictionary<string, object>
            {
                { "firstAbilityRequirement", new CreateSquadRequirement(firstRequirement) },
                { "firstAbilityCompute", FirstAbilityCompute },
                { "firstAbilityValue", FirstAbilityValue },
                { "comparison", Comparison },
                { "secondAbilityRequirement", new CreateSquadRequirement(secondRequirement) },
                { "secondAbilityCompute", SecondAbilityCompute },
                { "secondAbilityValue", SecondAbilityValue },
            };
        }
    }
}
