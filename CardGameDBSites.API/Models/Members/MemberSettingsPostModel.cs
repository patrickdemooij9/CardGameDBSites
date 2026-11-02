using System.Text.Json.Serialization;

namespace CardGameDBSites.API.Models.Members
{
    public class MemberSettingsPostModel
    {
        [JsonPropertyName("missingCopiesThreshold")]
        public int? MissingCopiesThreshold { get; set; }
    }
}
