using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.Business.DeckExport
{
    public class DeckJsonFile
    {
        [JsonPropertyName("metadata")]
        public DeckJsonMetaData Metadata { get; set; }

        [JsonPropertyName("leader")]
        public DeckJsonCard Leader { get; set; }

        [JsonPropertyName("base")]
        public DeckJsonCard Base { get; set; }

        [JsonPropertyName("deck")]
        public DeckJsonDeckCard[] Deck { get; set; }
    }

    public class DeckJsonMetaData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class DeckJsonCard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class DeckJsonDeckCard : DeckJsonCard
    {
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }
}
