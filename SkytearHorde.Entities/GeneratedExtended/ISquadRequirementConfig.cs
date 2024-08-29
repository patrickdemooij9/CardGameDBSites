using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Generated
{
    public partial interface ISquadRequirementConfig
    {
        ISquadRequirement GetRequirement();
        Dictionary<string, object> GetConfig();
    }
}
