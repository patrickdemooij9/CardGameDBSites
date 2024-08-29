using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ResultModels
{
    public class RecaptchaVerifyResultModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
