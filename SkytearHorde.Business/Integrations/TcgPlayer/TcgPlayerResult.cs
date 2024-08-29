using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerResult<T>
    {
        [JsonPropertyName("results")]
        public T[] Results { get; set; }
    }
}
