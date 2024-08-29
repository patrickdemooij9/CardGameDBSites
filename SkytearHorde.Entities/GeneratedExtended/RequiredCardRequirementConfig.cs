using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Requirements;

namespace SkytearHorde.Entities.Generated
{
    public partial class RequiredCardRequirementConfig
    {
        public ISquadRequirement GetRequirement()
        {
            return new RequiredCardRequirement(Card.Id);
        }

        public Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "requiredCardId", Card.Id }
            };
        }
    }
}
