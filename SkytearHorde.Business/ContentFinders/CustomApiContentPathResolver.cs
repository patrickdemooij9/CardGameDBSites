using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.ContentFinders
{
    public class CustomApiContentPathResolver : ApiContentPathResolver
    {
        private readonly ISiteService _siteService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly MetaCardPageService _metaCardPageService;

        public CustomApiContentPathResolver(IRequestRoutingService requestRoutingService, IApiPublishedContentCache apiPublishedContentCache, ISiteService siteService, IUmbracoContextFactory umbracoContextFactory, CardService cardService, CardPageService cardPageService, MetaCardPageService metaCardPageService) : base(requestRoutingService, apiPublishedContentCache)
        {
            _siteService = siteService;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
            _cardPageService = cardPageService;
            _metaCardPageService = metaCardPageService;
        }

        public override IPublishedContent? ResolveContentPath(string path)
        {
            // If path ends with an extension, ignore it
            var extension = Path.GetExtension(path);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            using var context = _umbracoContextFactory.EnsureUmbracoContext();

            var baseSystem = base.ResolveContentPath(path);
            if (baseSystem != null)
                return baseSystem;

            // decks/number
            path = path.EnsureStartsWith('/');
            
            var deckPage = TryGetDeckPage(path);
            if (deckPage != null) return deckPage;

            var setPage = TryGetSetPage(path, context);
            if (setPage != null) return setPage;

            var metaCardDetailPage = _metaCardPageService.GetDetailNode(path);
            if (metaCardDetailPage != null) return metaCardDetailPage;

            var cardPage = TryGetCardPage(path, context);
            return cardPage;
        }

        private IPublishedContent? TryGetDeckPage(string path)
        {
            var deckOverviews = _siteService.GetDeckOverviews();
            if (deckOverviews.Length == 0) return null;

            var foundDeckOverview = deckOverviews.FirstOrDefault(it => path.StartsWith(it.Url(mode: UrlMode.Relative)));
            if (foundDeckOverview is null)
                return null;

            return foundDeckOverview.FirstChild<DeckDetail>();
        }

        private IPublishedContent? TryGetCardPage(string path, UmbracoContextReference context)
        {
            var cardOverview = _siteService.GetRoot().FirstChild<CardOverview>();
            if (cardOverview is null || !path.StartsWith(cardOverview.Url(mode: UrlMode.Relative)))
                return null;

            var card = _cardPageService.GetBySegments(path.Split('/').Skip(2).ToArray());
            if (card is null)
                return null;

            return context.UmbracoContext.Content.GetById(card.VariantId);
        }

        private IPublishedContent? TryGetSetPage(string path, UmbracoContextReference context)
        {
            var setOverview = _siteService.GetSetOverview();
            if (setOverview is null || !path.StartsWith(setOverview.Url(mode: UrlMode.Relative)))
                return null;

            var urlSegment = path.Replace(setOverview.Url(mode: UrlMode.Relative), "");
            var sets = _cardService.GetAllSets();
            var set = sets.FirstOrDefault(it => it.UrlSegment!.Equals(urlSegment, StringComparison.InvariantCultureIgnoreCase));

            if (set is null)
                return null;

            return context.UmbracoContext.Content?.GetById(set.Id);
        }
    }
}
