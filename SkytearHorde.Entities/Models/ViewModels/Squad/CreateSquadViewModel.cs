using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slots")]
        public CreateSquadSlotViewModel[] Slots { get; set; }

        [JsonProperty("requirements")]
        public CreateSquadRequirement[] Requirements { get; set; }
    }
}
