using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateSquadSquadPostModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slots")]
        public CreateSquadSlotPostModel[] Slots { get; set; }

        public CreateSquadSquadPostModel()
        {
            Slots = Array.Empty<CreateSquadSlotPostModel>();
        }
    }
}
