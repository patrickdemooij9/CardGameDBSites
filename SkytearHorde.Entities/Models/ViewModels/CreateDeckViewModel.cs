using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CreateDeckViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("startingDeckId")]
        public Guid StartingDeckId { get; set; }

        [JsonPropertyName("name")]
        public string DeckName { get; set; }

        [JsonPropertyName("description")]
        public string DeckDescription { get; set; }

        [JsonPropertyName("initialCards")]
        public DeckCardViewModel[] InitialCards { get; set; }

        [JsonPropertyName("allCards")]
        public DeckCardViewModel[] AllCards { get; set; }

        public CreateDeckViewModel(Guid startingDeckId, string deckName)
        {
            StartingDeckId = startingDeckId;
            DeckName = deckName;

            InitialCards = Array.Empty<DeckCardViewModel>();
            AllCards = Array.Empty<DeckCardViewModel>();
        }
    }
}
