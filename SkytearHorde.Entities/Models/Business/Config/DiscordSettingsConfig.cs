namespace SkytearHorde.Entities.Models.Business.Config
{
    public class DiscordSettingsConfig
    {
        public required string Token { get; set; }
        public required string BaseUrl { get; set; }
        public string? FooterText { get; set; }
    }
}
