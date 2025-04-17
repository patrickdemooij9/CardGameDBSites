using SkytearHorde.Entities.Enums;
using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class DeckQueryPostModel
    {
        [JsonPropertyName("typeId")]
        public int? TypeId { get; set; }

        [JsonPropertyName("status")]
        public DeckStatus Status { get; set; }

        [JsonPropertyName("cards")]
        public int[] Cards { get; set; }

        [JsonPropertyName("take")]
        public int Take { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("userId")]
        public int? UserId { get; set; }

        [JsonPropertyName("orderBy")]
        public string? OrderBy { get; set; }

        public DeckQueryPostModel()
        {
            Cards = Array.Empty<int>();
        }
    }
}
