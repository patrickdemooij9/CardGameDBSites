namespace SkytearHorde.Business.Config
{
    public class CardGameSettingsConfig
    {
        public string SkillSetApiKey { get; set; }
        public string TcgPlayerApiKey { get; set; }
        public string TcgPlayerApiSecret { get; set; }
        public string SentryLink { get; set; }
        public string RecaptchaSecret { get; set; }
        public string CardReaderApiKey { get; set; }
        public string CloudflareApiToken { get; set; }
        public string CloudflareZoneId { get; set; }
        public string[] CloudflareHomepageUrls { get; set; } = [];
    }
}
