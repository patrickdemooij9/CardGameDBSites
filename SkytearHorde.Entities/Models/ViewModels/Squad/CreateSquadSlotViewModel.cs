using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadSlotViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("cardGroups")]
        public CreateSquadGroupingViewModel[] CardGroups { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("requirements")]
        public CreateSquadRequirement[] Requirements { get; set; }

        [JsonProperty("minCards")]
        public int MinCards { get; set; }

        [JsonProperty("maxCards")]
        public int MaxCards { get; set; }

        [JsonProperty("displaySize")]
        public string DisplaySize { get; set; }

        [JsonProperty("disableRemoval")]
        public required bool DisableRemoval { get; set; }

        [JsonProperty("numberMode")]
        public required bool NumberMode { get; set; }

        [JsonProperty("showIfTargetSlotIsFilled")]
        public int? ShowIfTargetSlotIsFilled { get; set; }

        [JsonProperty("additionalFilterRequirements")]
        public CreateSquadRequirement[] AdditionalFilterRequirements { get; set; }

        public CreateSquadSlotViewModel()
        {
            CardGroups = Array.Empty<CreateSquadGroupingViewModel>();
            AdditionalFilterRequirements = Array.Empty<CreateSquadRequirement>();
        }
    }
}
