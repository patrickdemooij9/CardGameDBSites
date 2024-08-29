using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadCharacterViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconUrls")]
        public string[] IconUrls { get; set; }

        [JsonProperty("abilities")]
        public CreateSquadAbilityViewModel[] Abilities { get; set; }

        [JsonProperty("images")]
        public CreateSquadCharacterImageViewModel[] Images { get; set; }

        [JsonProperty("teamRequirements")]
        public CreateSquadRequirement[] TeamRequirements { get; set; }

        [JsonProperty("squadRequirements")]
        public CreateSquadRequirement[] SquadRequirements { get; set; }

        [JsonProperty("slotRequirements")]
        public CreateSquadTargetSlotRequirement[] SlotRequirements { get; set; }
    }
}
