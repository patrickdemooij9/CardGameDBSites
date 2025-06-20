using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.Entities.Models.Business.Config
{
    public class SiteSettingsConfig
    {
        public required string MainColor { get; set; }
        public required string HoverMainColor { get; set; }
        public required string BorderColor { get; set; }
        public required string SiteName { get; set; }
        public required bool ShowLogin { get; set; }
        public required string NavigationLogoUrl { get; set; }
        public required bool TextColorWhite { get; set; }
        public required string FooterText { get; set; }
        public Link[] FooterLinks { get; set; }
        public NavigationItem[] Navigation { get; set; }
        public KeywordImageConfig[] Keywords { get; set; }
        public bool AllowPricing { get; set; }
        public RedditSettingsConfig? RedditSettings { get; set; }
        public SortOption[] SortOptions { get; set; }

        public SiteSettingsConfig()
        {
            Keywords = Array.Empty<KeywordImageConfig>();
            SortOptions = [];
            FooterLinks = [];
            Navigation = [];
        }
    }
}
