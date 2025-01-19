using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class SquadSlotAmountViewModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("config")]
        public Dictionary<string, object> Config { get; set; }
    }
}
