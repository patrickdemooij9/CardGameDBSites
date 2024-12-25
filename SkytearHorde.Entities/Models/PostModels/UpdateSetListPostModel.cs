using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class UpdateSetListPostModel
    {
        [JsonPropertyName("setId")]
        public int SetId { get; set; }

        [JsonPropertyName("listName")]
        public string ListName { get; set; }

        [JsonPropertyName("value")]
        public bool Value { get; set; }
    }
}
