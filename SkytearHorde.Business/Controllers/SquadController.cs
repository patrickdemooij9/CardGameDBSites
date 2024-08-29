using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System.Text.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Controllers
{
    public class SquadController : SurfaceController
    {
        private readonly DeckService _deckService;
        private readonly IMemberManager _memberManager;
        private readonly ISiteAccessor _siteAccessor;
        private readonly ISiteService _siteService;
        private readonly ViewRenderHelper _viewRenderHelper;

        public SquadController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, DeckService deckService, IMemberManager memberManager, ISiteAccessor siteAccessor, ISiteService siteService, ViewRenderHelper viewRenderHelper) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _deckService = deckService;
            _memberManager = memberManager;
            _siteAccessor = siteAccessor;
            _siteService = siteService;
            _viewRenderHelper = viewRenderHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSquad(string data, bool publish, int siteId)
        {
            _siteAccessor.SetSiteId(siteId);

            var postModel = JsonSerializer.Deserialize<CreateSquadPostModel>(data);
            if (postModel is null) return CurrentUmbracoPage();
            if (postModel.Name.Length > 60) return CurrentUmbracoPage();

            var currentUser = await _memberManager.GetCurrentMemberAsync();
            var deckId = _deckService.ProcessDeck(postModel, publish, currentUser != null ? int.Parse(currentUser.Id) : null);

            var deckOverview = _siteService.GetDeckOverview(postModel.TypeId);
            var returnUrl = deckOverview is null ? _siteService.GetRoot().Url() : $"{deckOverview.Url()}{deckId}";

            if (!publish)
            {
                var siteDecks = _siteService.GetRoot().FirstChild<AccountPage>()?.FirstChild<AccountDecks>()?.Url() ?? _siteService.GetRoot().Url();
                returnUrl = siteDecks;
            }

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> ToggleDeckLike(int siteId, int deckId, bool isAjax = false)
        {
            _siteAccessor.SetSiteId(siteId);

            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var deck = _deckService.Get(deckId);
            if (deck is null) return Unauthorized();

            var addedLike = _deckService.ToggleLikeDeck(deck, int.Parse(currentUser.Id));
            if (isAjax)
            {
                return new OkObjectResult(new AjaxResponse($"deck-like-{deckId}", _viewRenderHelper.RenderView("~/Views/Partials/components/deckLike.cshtml", new DeckLikeViewModel()
                {
                    DeckId = deckId,
                    AmountOfLikes = addedLike ? deck.AmountOfLikes + 1 : deck.AmountOfLikes - 1,
                    Like = addedLike,
                    IsAllowedToLike = true
                }).ToHtmlString()));
            }
            return RedirectToCurrentUmbracoPage();
        }
    }
}
