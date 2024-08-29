using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateSquadSlotPostModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("cards")]
        public CreateSquadCardPostModel[] Cards { get; set; }

        public CreateSquadSlotPostModel()
        {
            Cards = Array.Empty<CreateSquadCardPostModel>();
        }
    }
}
