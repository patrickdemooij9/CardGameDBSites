using CardGameDBSites.API.Models.Comments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/comments")]
    public class CommentsApiController : Controller
    {
        private readonly CommentService _commentService;
        private readonly MemberInfoService _memberInfoService;
        private readonly IMemberManager _memberManager;
        private readonly DeckService _deckService;

        public CommentsApiController(CommentService commentService,
            MemberInfoService memberInfoService,
            IMemberManager memberManager,
            DeckService deckService)
        {
            _commentService = commentService;
            _memberInfoService = memberInfoService;
            _memberManager = memberManager;
            _deckService = deckService;
        }

        [HttpGet("getByDeck")]
        [ProducesResponseType(typeof(CommentViewModel[]), 200)]
        public IActionResult GetByDeck(int deckId)
        {
            var deck = _deckService.Get(deckId);
            if (deck is null) return NotFound();

            var comments = _commentService.GetByDeck(deckId);
            return Ok(comments.Select(Map).ToArray());
        }

        [HttpPost("addDeckComment")]
        [Authorize(AuthenticationSchemes = "Jwt")]
        [ProducesResponseType(typeof(CommentViewModel), 200)]
        public async Task<IActionResult> AddDeckComment(CreateCommentPostModel model)
        {
            var deck = _deckService.Get(model.DeckId);
            if (deck is null) return NotFound();

            var member = await _memberManager.GetCurrentMemberAsync();
            if (member is null || !int.TryParse(member.Id, out var memberId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Comment)) return BadRequest("Comment cannot be empty");

            return Ok(Map(_commentService.AddNewDeckComment(model.DeckId, model.Comment, memberId)));
        }

        [HttpDelete("deleteDeckComment")]
        [Authorize(AuthenticationSchemes = "Jwt")]
        public async Task<IActionResult> DeleteDeckComment(int commentId)
        {
            var comment = _commentService.GetDeckComment(commentId);
            if (comment is null) return NotFound();

            var deck = _deckService.Get(comment.DeckId);
            if (deck is null) return NotFound();

            var member = await _memberManager.GetCurrentMemberAsync();
            if (member is null || !int.TryParse(member.Id, out var memberId) || comment.CreatedBy != memberId) return Unauthorized();

            _commentService.DeleteDeckComment(commentId);
            return Ok();
        }

        private CommentViewModel Map(DeckComment comment)
        {
            return new CommentViewModel
            {
                Id = comment.Id,
                Comment = comment.Comment,
                CreatedAt = comment.CreatedAt,
                CreatedById = comment.CreatedBy,
                CreatedByName = _memberInfoService.Get(comment.CreatedBy)?.DisplayName ?? "Unknown"
            };
        }
    }
}
