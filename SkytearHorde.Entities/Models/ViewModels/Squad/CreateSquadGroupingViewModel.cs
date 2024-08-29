using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadGroupingViewModel
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("sortBy")]
        public string SortBy { get; set; }

        [JsonPropertyName("cardIds")]
        public CreateSquadCardAmountViewModel[] CardIds { get; set; }

        [JsonPropertyName("requirements")]
        public CreateSquadRequirement[] Requirements { get; set; }

        public CreateSquadGroupingViewModel()
        {
            CardIds = Array.Empty<CreateSquadCardAmountViewModel>();
            Requirements = Array.Empty<CreateSquadRequirement>();
        }
    }
}
