using NPoco.Expressions;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.ContentFinders
{
    public class CardPageContentFinder : IContentFinder
    {
        private readonly CardPageService _cardPageService;
        private readonly IFileService _fileService;
        private readonly ISiteService _siteService;
        private readonly CardService _cardService;
        private readonly IRequestCache _requestCache;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CardPageContentFinder(CardPageService cardPageService, IFileService fileService, ISiteService siteService,
            CardService cardService, IRequestCache requestCache, IUmbracoContextFactory umbracoContextFactory)
        {
            _cardPageService = cardPageService;
            _fileService = fileService;
            _siteService = siteService;
            _cardService = cardService;
            _requestCache = requestCache;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {
            var root = _siteService.GetRoot();
            var cardOverview = root.FirstChild<CardOverview>();
            if (cardOverview is null)
                return Task.FromResult(false);

            if (!request.Uri.AbsolutePath.StartsWith(cardOverview.Url()))
                return Task.FromResult(false);

            var segments = request.Uri.AbsolutePath.TrimStart(cardOverview.Url()).Split('/');

            var card = FindCard(segments, out var redirectCard);
            if (card is null)
                return Task.FromResult(false);

            if (redirectCard)
            {
                request.SetRedirect(_cardPageService.GetUrl(card), 301);
                return Task.FromResult(true);
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var umbracoCard = ctx.UmbracoContext.Content?.GetById(card.BaseId);
            if (umbracoCard is null)
                return Task.FromResult(false);

            var template = _fileService.GetTemplate("cardDetail");
            if (template is null)
                return Task.FromResult(false);

            request.SetPublishedContent(umbracoCard);
            request.SetTemplate(template);
            _requestCache.Set("card", card);
            return Task.FromResult(true);
        }

        private Card? FindCard(string[] segments, out bool redirectCard)
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
    }
}
