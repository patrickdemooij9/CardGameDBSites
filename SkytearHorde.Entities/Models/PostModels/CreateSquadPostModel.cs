using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateSquadPostModel
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("typeId")]
        public int TypeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("squads")]
        public CreateSquadSquadPostModel[] Squads { get; set; }

        public CreateSquadPostModel()
        {
            Squads = Array.Empty<CreateSquadSquadPostModel>();
        }
    }
}
