using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class SizeSquadRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new SizeSquadRequirement(Minimum, Maximum);
        }
        public Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "min", Minimum },
                { "max", Maximum }
            };
        }
    }
}
