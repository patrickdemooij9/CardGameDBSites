using SkytearHorde.Entities.Generated;

namespace CardGameDBSites.API.Models.Requirements
{
    public class RequirementApiModel
    {
        public string Alias { get; set; }
        public Dictionary<string, object> Config { get; set; }

        public RequirementApiModel(ISquadRequirementConfig requirementConfig)
        {
            Alias = requirementConfig.GetRequirement().Alias;
            Config = requirementConfig.GetConfig();
        }
    }
}
