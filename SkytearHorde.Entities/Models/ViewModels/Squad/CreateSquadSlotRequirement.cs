using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadSlotRequirement
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
