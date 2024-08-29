using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateDeckPostModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("startingDeckId")]
        public Guid StartingDeckId { get; set; }

        [JsonPropertyName("name")]
        public string DeckName { get; set; }

        [JsonPropertyName("description")]
        public string DeckDescription { get; set; }

        [JsonPropertyName("cards")]
        public CreateDeckItemModel[] Cards { get; set; }

        public CreateDeckPostModel()
        {
            Cards = Array.Empty<CreateDeckItemModel>();
        }
    }
}
