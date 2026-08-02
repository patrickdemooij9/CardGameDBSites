using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    /// <summary>
    /// Resolves MetaCardDetail pages from a URL. Like card pages, there is no per-card content node:
    /// a single MetaCardDetail template node lives under a MetaCardOverview, and the card is derived
    /// from the URL segments after the overview. Shared by the content path resolver (to route the
    /// request) and the meta API (to tell the frontend which card the page is about).
    /// Requires an ambient UmbracoContext for URL resolution.
    /// </summary>
    public class MetaCardPageService
    {
        private readonly ISiteService _siteService;
        private readonly CardPageService _cardPageService;

        public MetaCardPageService(ISiteService siteService, CardPageService cardPageService)
        {
            _siteService = siteService;
            _cardPageService = cardPageService;
        }

        /// <summary>The MetaCardOverview whose URL is the longest prefix of the path, or null.</summary>
        public MetaCardOverview? FindOverviewForPath(string relativePath)
        {
            var normalized = relativePath.EnsureStartsWith('/');
            return _siteService.GetRoot().DescendantsOrSelf<MetaCardOverview>()
                .Where(o => normalized.StartsWith(o.Url(mode: UrlMode.Relative), StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(o => o.Url(mode: UrlMode.Relative).Length)
                .FirstOrDefault();
        }

        /// <summary>The card the given meta-detail path refers to, or null when it doesn't resolve.</summary>
        public Card? ResolveCard(string relativePath)
        {
            var normalized = relativePath.EnsureStartsWith('/');
            var overview = FindOverviewForPath(normalized);
            return overview is null ? null : ResolveCard(overview, normalized);
        }

        /// <summary>The MetaCardDetail template node to render for the path, or null when no card matches.</summary>
        public IPublishedContent? GetDetailNode(string relativePath)
        {
            var normalized = relativePath.EnsureStartsWith('/');
            var overview = FindOverviewForPath(normalized);
            if (overview is null) return null;

            return ResolveCard(overview, normalized) is null ? null : overview.FirstChild<MetaCardDetail>();
        }

        /// <summary>
        /// The meta detail URL for a card if it qualifies as a leader on any MetaCardOverview (i.e. it
        /// satisfies that overview's cardRequirement), or null. Used to link from the card page to its
        /// meta page. Requires an ambient UmbracoContext.
        /// </summary>
        public string? GetMetaUrlForCard(Card card)
        {
            foreach (var overview in _siteService.GetRoot().DescendantsOrSelf<MetaCardOverview>())
            {
                var requirements = overview.CardRequirement.ToItems<ISquadRequirementConfig>().ToArray();
                if (requirements.Length == 0) continue;

                if (requirements.All(r => r.GetRequirement().IsValid([card])))
                {
                    return $"{overview.Url(mode: UrlMode.Relative)}{card.UrlSegment}";
                }
            }
            return null;
        }

        private Card? ResolveCard(MetaCardOverview overview, string normalizedPath)
        {
            var prefix = overview.Url(mode: UrlMode.Relative);
            var remainder = normalizedPath[prefix.Length..].Trim('/');
            return string.IsNullOrWhiteSpace(remainder) ? null : _cardPageService.GetByUrl(remainder);
        }
    }
}
