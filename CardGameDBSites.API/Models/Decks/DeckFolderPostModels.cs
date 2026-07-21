using System.Text.Json.Serialization;

namespace CardGameDBSites.API.Models.Decks
{
    public class CreateDeckFolderPostModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class UpdateDeckFolderPostModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class MoveDecksPostModel
    {
        /// <summary>Target folder. When null the decks are removed from any folder ("unfiled").</summary>
        [JsonPropertyName("folderId")]
        public int? FolderId { get; set; }

        [JsonPropertyName("deckIds")]
        public int[] DeckIds { get; set; } = Array.Empty<int>();
    }
}
