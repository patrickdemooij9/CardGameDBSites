using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Middleware;

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
        private readonly CardService _cardService;

        private readonly ISiteAccessor _siteAccessor;


        public MetaCardPageService(ISiteService siteService,
            CardPageService cardPageService,
            CardService cardService,
            ISiteAccessor siteAccessor)
        {
            _siteService = siteService;

            _cardPageService = cardPageService;
            _cardService = cardService;

            _siteAccessor = siteAccessor;
        }

        /// <summary>The card the given meta-detail path refers to, or null when it doesn't resolve.</summary>
        public Card? ResolveCard(string relativePath)
        {
            var normalized = relativePath.EnsureStartsWith('/');
            var overview = _siteService.GetMetaCardOverview();
            return overview is null ? null : ResolveCard(overview, normalized);
        }

        /// <summary>The MetaCardDetail template node to render for the path, or null when no card matches.</summary>
        public IPublishedContent? GetDetailNode(string relativePath)
        {
            var normalized = relativePath.EnsureStartsWith('/');
            var overview = _siteService.GetMetaCardOverview();
            if (overview is null) return null;

            return ResolveCard(overview, normalized) is null ? null : overview.FirstChild<MetaCardDetail>();
        }

        public Card[] GetCardsForOverview()
        {
            var overview = _siteService.GetMetaCardOverview();
            if (overview is null) return Array.Empty<Card>();

            var filters = new List<CardSearchFilterClause>
            {
                new() {
                    Filters = [new() {
                        Alias = "Usage",
                        Mode = CardSearchFilterMode.Higher,
                        Values = ["0.1"]
                    }]
                }
            };
            foreach (var filter in overview.CardRequirement.ToItems<ISquadRequirementConfig>())
            {
                if (filter is EqualAbilitySquadRequirementConfig equal)
                {
                    filters.Add(new CardSearchFilterClause
                    {
                        Filters = [new CardSearchFilter
                        {
                            Alias = equal.Ability!.Name,
                            Mode = CardSearchFilterMode.Contains,
                            Values = equal.Values?.ToArray() ?? []
                        }]
                    });
                }
            }

            var cards = _cardService.Search(new CardSearchQuery(100, _siteAccessor.GetSiteId())
            {
                FilterClauses = filters,
                VariantTypeIds = [0]
            }, out _);
            return cards;
        }

        public string? GetMetaUrlForCard(Card card)
        {
            var overview = _siteService.GetMetaCardOverview();
            if (overview is null) return null;
            var requirements = overview.CardRequirement.ToItems<ISquadRequirementConfig>().ToArray();
            if (requirements.Length == 0 || requirements.All(r => r.GetRequirement().IsValid([card])))
            {
                return $"{overview.Url(mode: UrlMode.Relative)}{card.UrlSegment}";
            }
            return null;
        }

        private Card? ResolveCard(MetaCardOverview overview, string normalizedPath)
        {
            var prefix = overview.Url(mode: UrlMode.Relative);
            var remainder = normalizedPath[prefix.Length..].Trim('/');
            return string.IsNullOrWhiteSpace(remainder) ? null : _cardPageService.GetByUrl(remainder, includeVariants: false);
        }
    }
}
