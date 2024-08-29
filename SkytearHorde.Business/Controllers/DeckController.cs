using FormBuilder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.FormBuilders;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class DeckController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly DeckService _deckService;
        private readonly FormValidationService _formValidationService;
        private readonly CommentService _commentService;

        public DeckController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IMemberManager memberManager, DeckService deckService, FormValidationService formValidationService, CommentService commentService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _deckService = deckService;
            _formValidationService = formValidationService;
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDeck(int deckId)
        {
            var deck = _deckService.Get(deckId, DeckStatus.None);
            if (deck is null) return NotFound();

            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            if (deck.CreatedBy != currentUser)
                return NotFound();

            _deckService.DeleteDeck(deck);
            return RedirectToCurrentUmbracoPage();
        }

        public async Task<IActionResult> AddComment(DeckCommentForm model)
        {
            if (!_formValidationService.Validate(ModelState, new DeckCommentFormBuilder(), model))
            {
                return CurrentUmbracoPage();
            }

            var deck = _deckService.Get(model.SourceId);
            if (deck is null) return CurrentUmbracoPage();

            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            var comment = _commentService.AddNewDeckComment(model.SourceId, model.Comment, currentUser);

            return Redirect($"{UmbracoContext.OriginalRequestUrl.PathAndQuery}#comment-{comment.Id}");
        }

        public async Task<IActionResult> DeleteComment(int sourceId, int commentId)
        {
            var deck = _deckService.Get(sourceId);
            if (deck is null) return CurrentUmbracoPage();

            var comment = _commentService.GetDeckComment(commentId);
            if (comment is null) return CurrentUmbracoPage();

            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            if (currentUser != comment.CreatedBy) return CurrentUmbracoPage();

            _commentService.DeleteDeckComment(commentId);

            return Redirect(UmbracoContext.OriginalRequestUrl.PathAndQuery);
        }
    }
}
