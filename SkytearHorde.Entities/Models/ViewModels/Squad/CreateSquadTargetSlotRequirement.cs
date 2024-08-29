using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadTargetSlotRequirement
    {
        [JsonProperty("slotId")]
        public int SlotId { get; set; }

        [JsonProperty("requirements")]
        public CreateSquadRequirement[] Requirements { get; set; }
    }
}
