using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class EqualAbilitySquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new EqualValueRequirement(Ability.Name, ValueList?.ToArray() ?? Array.Empty<string>());
        }

        public Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "ability", Ability.Name },
                { "values", ValueList?.ToArray() ?? Array.Empty<string>() }
            };
        }
    }
}
