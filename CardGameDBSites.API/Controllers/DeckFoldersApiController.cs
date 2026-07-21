using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.Decks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/deckFolders")]
    [JwtAuthorization]
    [ApiExplorerSettings(GroupName = "DeckFolders")]
    public class DeckFoldersApiController : Controller
    {
        private readonly DeckFolderService _deckFolderService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IMemberManager _memberManager;

        public DeckFoldersApiController(
            DeckFolderService deckFolderService,
            ISiteAccessor siteAccessor,
            IMemberManager memberManager)
        {
            _deckFolderService = deckFolderService;
            _siteAccessor = siteAccessor;
            _memberManager = memberManager;
        }

        [HttpGet("getByUser")]
        [ProducesResponseType(typeof(DeckFolderApiModel[]), 200)]
        public async Task<IActionResult> GetByUser()
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var userId = int.Parse(currentUser.Id);
            var siteId = _siteAccessor.GetSiteId();

            var counts = _deckFolderService.GetDeckCounts(userId, siteId);
            var folders = _deckFolderService.GetByUser(userId, siteId)
                .OrderBy(it => it.Name)
                .Select(it => new DeckFolderApiModel(it, counts.TryGetValue(it.Id, out var count) ? count : 0))
                .ToArray();

            return Ok(folders);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Create([FromBody] CreateDeckFolderPostModel model)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required");

            var id = _deckFolderService.Create(model.Name.Trim(), model.Description, int.Parse(currentUser.Id), _siteAccessor.GetSiteId());
            return Ok(id);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateDeckFolderPostModel model)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required");

            var updated = _deckFolderService.Update(model.Id, int.Parse(currentUser.Id), model.Name.Trim(), model.Description);
            if (!updated) return NotFound();

            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            var deleted = _deckFolderService.Delete(id, int.Parse(currentUser.Id), _siteAccessor.GetSiteId());
            if (!deleted) return NotFound();

            return Ok();
        }

        [HttpPost("moveDecks")]
        public async Task<IActionResult> MoveDecks([FromBody] MoveDecksPostModel model)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();
            if (currentUser is null) return Unauthorized();

            if (model.DeckIds.Length == 0) return Ok();

            var moved = _deckFolderService.MoveDecks(model.FolderId, int.Parse(currentUser.Id), _siteAccessor.GetSiteId(), model.DeckIds);
            if (!moved) return BadRequest("Invalid target folder");

            return Ok();
        }
    }
}
