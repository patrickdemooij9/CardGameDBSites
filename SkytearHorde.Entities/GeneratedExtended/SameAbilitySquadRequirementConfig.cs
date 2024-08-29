using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class SameAbilitySquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new SameValueRequirement(Ability.Name);
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
