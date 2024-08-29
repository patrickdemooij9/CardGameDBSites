using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class MarkdownPreviewPostModel
    {
        [JsonPropertyName("markdown")]
        public string Markdown { get; set; }
    }
}
