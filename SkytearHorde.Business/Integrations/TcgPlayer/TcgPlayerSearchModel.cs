using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerSearchModel
    {
        [JsonPropertyName("sort")]
		public string Sort { get; set; }

        [JsonPropertyName("limit")]
		public int Limit { get; set; }

        [JsonPropertyName("offset")]
		public int Offset { get; set; }

        [JsonPropertyName("filters")]
        public TcgPlayerSearchFilterModel[] Filters { get; set; }
    }
}
