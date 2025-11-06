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

        public CustomApiContentPathResolver(IRequestRoutingService requestRoutingService, IApiPublishedContentCache apiPublishedContentCache, ISiteService siteService, IUmbracoContextFactory umbracoContextFactory, CardService cardService, CardPageService cardPageService) : base(requestRoutingService, apiPublishedContentCache)
        {
            _siteService = siteService;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
            _cardPageService = cardPageService;
        }

        public override IPublishedContent? ResolveContentPath(string path)
        {
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

            var card = FindCard(path.Split('/').Skip(2).ToArray(), out var _);
            if (card is null)
                return null;

            return context.UmbracoContext.Content.GetById(card.BaseId);
        }

        private Entities.Models.Business.Card? FindCard(string[] segments, out bool redirectCard)
        {
            redirectCard = false;
            if (segments.Length == 0) return null;

            var sets = _cardService.GetAllSets().Where(it => !string.IsNullOrWhiteSpace(it.SetCode));
            var potentialSet = sets.FirstOrDefault(it => it.SetCode?.Equals(segments[0], StringComparison.InvariantCultureIgnoreCase) is true);

            string urlSegment;
            if (potentialSet is null)
            {
                urlSegment = segments.Length == 1 ? segments[0] : $"{segments[0]}/{segments[1]}";
            }
            else
            {
                urlSegment = segments.Length == 2 ? segments[1] : $"{segments[1]}/{segments[2]}";
            }
            var card = _cardPageService.GetByUrl(urlSegment, potentialSet?.SetCode);
            if (card is null) return null;

            var cardSet = sets.FirstOrDefault(it => it.Id == card.SetId);
            redirectCard = !string.IsNullOrWhiteSpace(cardSet?.SetCode) && potentialSet is null;

            return card;
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
