using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.FormBuilders;
using SkytearHorde.Business.Services;
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
    public class CardController : SurfaceController
    {
        private readonly CommentService _commentService;
        private readonly CardService _cardService;
        private readonly IMemberManager _memberManager;

        public CardController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, CommentService commentService, CardService cardService, IMemberManager memberManager) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commentService = commentService;
            _cardService = cardService;
            _memberManager = memberManager;
        }

        public async Task<IActionResult> AddComment(CardCommentForm model)
        {
            var card = _cardService.Get(model.SourceId);
            if (card is null) return CurrentUmbracoPage();

            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            var comment = _commentService.AddNewCardComment(model.SourceId, model.Comment, currentUser);

            return Redirect($"{UmbracoContext.OriginalRequestUrl.PathAndQuery}#comment-{comment.Id}");
        }

        public async Task<IActionResult> DeleteComment(int sourceId, int commentId)
        {
            var card = _cardService.Get(sourceId);
            if (card is null) return CurrentUmbracoPage();

            var comment = _commentService.GetCardComment(commentId);
            if (comment is null) return CurrentUmbracoPage();

            var currentUser = int.Parse((await _memberManager.GetCurrentMemberAsync()).Id);
            if (currentUser != comment.CreatedBy) return CurrentUmbracoPage();

            _commentService.DeleteCardComment(commentId);

            return Redirect(UmbracoContext.OriginalRequestUrl.PathAndQuery);
        }
    }
}
