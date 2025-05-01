using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.ContentFinders
{
    public class DeckApiContentPathResolver : ApiContentPathResolver
    {
        private readonly ISiteService _siteService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public DeckApiContentPathResolver(IRequestRoutingService requestRoutingService, IApiPublishedContentCache apiPublishedContentCache, ISiteService siteService, IUmbracoContextFactory umbracoContextFactory) : base(requestRoutingService, apiPublishedContentCache)
        {
            _siteService = siteService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override IPublishedContent? ResolveContentPath(string path)
        {
            // decks/number
            using var _ = _umbracoContextFactory.EnsureUmbracoContext();

            path = path.EnsureStartsWith('/');
            var deckOverviews = _siteService.GetDeckOverviews();
            if (deckOverviews.Length == 0) return base.ResolveContentPath(path);

            var foundDeckOverview = deckOverviews.FirstOrDefault(it => path.StartsWith(it.Url()));
            if (foundDeckOverview is null)
                return base.ResolveContentPath(path);

            return foundDeckOverview.FirstChild<DeckDetail>();
        }
    }
}
