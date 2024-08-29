using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class UniqueAbilitySquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new UniqueValueRequirement(Ability.Name);
        }
        public Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "ability", Ability.Name }
            };
        }
    }
}
