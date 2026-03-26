using CardGameDBSites.API.Models.Comments;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/comments")]
    public class CommentsController : Controller
    {
        private readonly CommentService _commentService;
        private readonly MemberInfoService _memberInfoService;

        public CommentsController(CommentService commentService, MemberInfoService memberInfoService)
        {
            _commentService = commentService;
            _memberInfoService = memberInfoService;
        }

        [HttpGet("getByDeck")]
        [ProducesResponseType(typeof(CommentViewModel[]), 200)]
        public IActionResult GetByDeck(int deckId)
        {
            var comments = _commentService.GetByDeck(deckId);
            return Ok(comments.Select(it => new CommentViewModel
            {
                Id = it.Id,
                Comment = it.Comment,
                CreatedAt = it.CreatedAt,
                CreatedById = it.CreatedBy,
                CreatedByName = _memberInfoService.Get(it.CreatedBy)?.DisplayName ?? "Unknown"
            }).ToArray());
        }
    }
}
