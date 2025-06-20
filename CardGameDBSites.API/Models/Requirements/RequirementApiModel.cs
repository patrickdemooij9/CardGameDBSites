using SkytearHorde.Entities.Generated;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Models.Requirements
{
    public class RequirementApiModel
    {
        public string Alias { get; set; }
        public RestrictionType RestrictionType { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Config { get; set; }

        public RequirementApiModel(ISquadRequirementConfig requirementConfig)
        {
            Alias = requirementConfig.GetRequirement().Alias;
            RestrictionType = (RestrictionType)Enum.Parse(typeof(RestrictionType), requirementConfig.RestrictionType.IfNullOrWhiteSpace("Hard"));
            Config = requirementConfig.GetConfig();
            ErrorMessage = requirementConfig.ErrorMessage;
        }
    }
}
