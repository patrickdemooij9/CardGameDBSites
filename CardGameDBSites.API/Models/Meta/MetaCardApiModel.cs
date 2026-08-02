using CardGameDBSites.API.Models;

namespace CardGameDBSites.API.Models.Meta
{
    /// <summary>
    /// The card a MetaCardDetail page is about, resolved from the page URL. Just enough for the page's
    /// header and to key its stats/deck lookups; the link back to the real card page is <see cref="UrlSegment"/>.
    /// </summary>
    public class MetaCardApiModel
    {
        public int BaseId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string UrlSegment { get; set; } = string.Empty;
        public ImageCropsApiModel? ImageUrl { get; set; }
    }
}
