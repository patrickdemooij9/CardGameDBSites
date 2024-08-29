using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadTeamViewModel
    {
        public int SiteId { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("typeId")]
        public int TypeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [JsonProperty("squads")]
        public CreateSquadViewModel[] Squads { get; set; }

        [JsonProperty("allCharacters")]
        public CreateSquadCharacterViewModel[] AllCharacters { get; set; }

        [JsonProperty("ownedCharacters")]
        public int[] OwnedCharacters { get; set; }

        [JsonProperty("requirements")]
        public CreateSquadRequirement[] Requirements { get; set; }

        [JsonProperty("preselectFirstSlot")]
        public bool PreselectFirstSlot { get; set; }

        [JsonProperty("hasDynamicSlot")]
        public bool HasDynamicSlot { get; set; }
    }
}
