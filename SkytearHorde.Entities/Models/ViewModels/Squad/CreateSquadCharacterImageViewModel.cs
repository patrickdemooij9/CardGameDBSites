using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadCharacterImageViewModel
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("isPrimaryImage")]
        public bool IsPrimaryImage { get; set; }
    }
}
