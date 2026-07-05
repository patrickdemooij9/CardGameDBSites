namespace SkytearHorde.Entities.Models.Business.Config
{
    public class DiscordSettingsConfig
    {
        public required string Token { get; set; }
        public required string BaseUrl { get; set; }
        public string? FooterText { get; set; }

        /// <summary>
        /// Discord channel ID to monitor for card reveal images.
        /// Configure via appsettings: CardImport:RevealChannels:{siteId}
        /// </summary>
        public ulong? RevealChannelId { get; set; }
    }
}
