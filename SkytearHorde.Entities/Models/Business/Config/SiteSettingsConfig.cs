using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.Business.Config
{
    public class SiteSettingsConfig
    {
        public required string MainColor { get; set; }
        public required string HoverMainColor { get; set; }
        public required string BorderColor { get; set; }
        public KeywordImageConfig[] Keywords { get; set; }
        public bool AllowPricing { get; set; }
        public RedditSettingsConfig? RedditSettings { get; set; }
        public SortOption[] SortOptions { get; set; }

        public SiteSettingsConfig()
        {
            Keywords = Array.Empty<KeywordImageConfig>();
            SortOptions = [];
        }
    }
}
