using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateDeckItemModel
    {
        [JsonPropertyName("cardId")]
        public int CardId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
