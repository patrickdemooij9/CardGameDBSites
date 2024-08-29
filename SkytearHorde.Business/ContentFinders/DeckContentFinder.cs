using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using System.Web;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace SkytearHorde.Business.ContentFinders
{
    public class DeckContentFinder : IContentFinder
    {
        private readonly DeckService _deckService;
        private readonly IFileService _fileService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly IRequestCache _requestCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeckContentFinder(DeckService deckService, IFileService fileService, IUmbracoContextFactory umbracoContextFactory, ISiteService siteService, IRequestCache requestCache, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _deckService = deckService;
            _fileService = fileService;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _requestCache = requestCache;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {
            var deckOverviews = _siteService.GetDeckOverviews();
            if (deckOverviews.Length == 0) return false;

            var foundDeckOverview = deckOverviews.FirstOrDefault(it => request.Uri.AbsolutePath.StartsWith(it.Url()));
            if (foundDeckOverview is null)
                return false;

            if (!int.TryParse(request.Uri.Segments[^1], out var deckId))
                return false;

            var deck = await GetDeck(deckId);
            if (deck is null)
                return false;

            var typeId = (foundDeckOverview.SquadSettings as SquadSettings)?.TypeID ?? 1;
            if (deck.TypeId != typeId)
                return false;

            var template = _fileService.GetTemplate("deckDetail");
            if (template is null)
                return false;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var deckDetailPage = foundDeckOverview.FirstChild<DeckDetail>();
            if (deckDetailPage is null)
                return false;

            request.SetPublishedContent(deckDetailPage);
            request.SetTemplate(template);
            _requestCache.Set("Deck", deck);
            return true;
        }

        private async Task<Deck?> GetDeck(int id)
        {
            var publishedDeck = _deckService.Get(id);

            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            var currentUserResult = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            if (currentUserResult.Succeeded)
            {
                var savedDeck = _deckService.Get(id, DeckStatus.Saved);
                if (savedDeck != null &&
                    savedDeck.CreatedBy == currentUserResult.Principal.Identity?.GetUserId<int>() &&
                    savedDeck.CreatedDate > (publishedDeck?.CreatedDate ?? DateTime.MinValue))
                {
                    publishedDeck = savedDeck;
                }
            }

            return publishedDeck;
        }
    }
}
