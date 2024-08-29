using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class NotEqualAbilitySquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new NotEqualValueRequirement(Ability.Name, Values?.ToArray() ?? Array.Empty<string>());
        }
        public Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "ability", Ability.Name },
                { "values", Values?.ToArray() ?? Array.Empty<string>() }
            };
        }
    }
}
