using Newtonsoft.Json;
using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadRequirement
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("config")]
        public Dictionary<string, object> Config { get; set; }

        public CreateSquadRequirement()
        {

        }

        public CreateSquadRequirement(ISquadRequirementConfig config)
        {
            Type = config.GetRequirement().Alias;
            Config = config.GetConfig();
        }
    }
}
