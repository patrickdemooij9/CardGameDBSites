using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerSearchFilterModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("values")]
        public string[] Values { get; set; }
    }
}
