using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Tournaments
{
    /// <summary>
    /// Represents a card from a Melee.GG decklist detail response.
    /// </summary>
    public class MeleeDeckCard
    {
        [JsonPropertyName("l")]
        public string Lookup { get; set; } = string.Empty;

        [JsonPropertyName("n")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("s")]
        public string? Subtitle { get; set; }

        [JsonPropertyName("q")]
        public int Quantity { get; set; }

        [JsonPropertyName("c")]
        public int Component { get; set; } // 0 = Main deck, 99 = Sideboard

        [JsonPropertyName("t")]
        public string Type { get; set; } = string.Empty;
    }
}
