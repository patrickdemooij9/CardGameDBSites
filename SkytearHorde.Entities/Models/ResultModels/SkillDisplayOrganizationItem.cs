using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ResultModels
{
    public class SkillDisplayOrganizationItem
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("granted")]
        public string Granted { get; set; }

        [JsonPropertyName("skillUid")]
        public int SkillUid { get; set; }

        [JsonPropertyName("skillUUid")]
        public Guid SkillUUid { get; set; }

        [JsonPropertyName("skill")]
        public string Skill { get; set; }

        [JsonPropertyName("domainTag")]
        public string DomainTag { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("certifier")]
        public string Certifier { get; set; }

        [JsonPropertyName("organisation")]
        public string Organisation { get; set; }

        [JsonPropertyName("campaign")]
        public string Campaign { get; set; }

        [JsonPropertyName("skillSetUid")]
        public int SkillSetUid { get; set; }

        [JsonPropertyName("skillSetName")]
        public string SkillSetName { get; set; }
    }
}
