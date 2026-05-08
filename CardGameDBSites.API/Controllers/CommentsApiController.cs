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
using SkytearHorde.Entities.Models;
using CardGameDBSites.API.Attributes;

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
        private readonly CardService _cardService;

        public CommentsApiController(CommentService commentService,
            MemberInfoService memberInfoService,
            IMemberManager memberManager,
            DeckService deckService,
            CardService cardService)
        {
            _commentService = commentService;
            _memberInfoService = memberInfoService;
            _memberManager = memberManager;
            _deckService = deckService;
            _cardService = cardService;
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
        [JwtAuthorization]
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
        [JwtAuthorization]
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

        [HttpGet("getByCard")]
        [ProducesResponseType(typeof(CommentViewModel[]), 200)]
        public IActionResult GetByCard(int cardId)
        {
            var card = _cardService.Get(cardId);
            if (card is null) return NotFound();

            var comments = _commentService.GetByCard(cardId);
            return Ok(comments.Select(MapCardComment).ToArray());
        }

        [HttpPost("addCardComment")]
        [JwtAuthorization]
        [ProducesResponseType(typeof(CommentViewModel), 200)]
        public async Task<IActionResult> AddCardComment(CreateCommentPostModel model)
        {
            var card = _cardService.Get(model.CardId);
            if (card is null) return NotFound();

            var member = await _memberManager.GetCurrentMemberAsync();
            if (member is null || !int.TryParse(member.Id, out var memberId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Comment)) return BadRequest("Comment cannot be empty");

            return Ok(MapCardComment(_commentService.AddNewCardComment(model.CardId, model.Comment, memberId)));
        }

        [HttpDelete("deleteCardComment")]
        [JwtAuthorization]
        public async Task<IActionResult> DeleteCardComment(int commentId)
        {
            var comment = _commentService.GetCardComment(commentId);
            if (comment is null) return NotFound();

            var card = _cardService.Get(comment.CardId);
            if (card is null) return NotFound();

            var member = await _memberManager.GetCurrentMemberAsync();
            if (member is null || !int.TryParse(member.Id, out var memberId) || comment.CreatedBy != memberId) return Unauthorized();

            _commentService.DeleteCardComment(commentId);
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

        private CommentViewModel MapCardComment(CardComment comment)
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
