using SkytearHorde.Entities.Enums;
using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CardOverviewPostModel
    {
        public OverviewDataSourceKey DataSourceKey { get; set; }

        public Dictionary<string, string[]> Filters { get; set; }

        [JsonPropertyName("search")]
        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public int PageId { get; set; }

        [JsonPropertyName("pageNumber")]
        public int? PageNumber { get; set; }

        public CardOverviewPostModel()
        {
            Filters = new Dictionary<string, string[]>();
        }
    }
}
