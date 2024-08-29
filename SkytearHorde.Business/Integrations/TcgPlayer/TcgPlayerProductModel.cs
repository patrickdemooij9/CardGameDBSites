using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerProductModel
    {
        [JsonPropertyName("productId")]
        public long ProductId { get; set; }

        [JsonPropertyName("cleanName")]
        public string CleanName { get; set; }

        [JsonPropertyName("groupId")]
        public int GroupId { get; set; }
    }
}
