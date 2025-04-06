using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Models.Business
{
    public class ComputedRequirementConfig
    {
        public required ISquadRequirement FirstAbilityRequirement { get; set; }
        public required ComputedType FirstAbilityType { get; set; }
        public required string FirstAbilityValue { get; set; }

        public required ComputedComparisonType Comparison { get; set; }

        public required ISquadRequirement SecondAbilityRequirement { get; set; }
        public required ComputedType SecondAbilityType { get; set; }
        public required string SecondAbilityValue { get; set; }
    }
}
