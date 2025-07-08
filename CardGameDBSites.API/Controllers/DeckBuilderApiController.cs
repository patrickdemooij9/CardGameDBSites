using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Cms.Core.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/deckbuilder")]
    public class DeckBuilderApiController : Controller
    {
        private readonly DeckService _deckService;
        private readonly IMemberManager _memberManager;

        public DeckBuilderApiController(DeckService deckService, IMemberManager memberManager)
        {
            _deckService = deckService;
            _memberManager = memberManager;
        }

        [HttpPost("submit")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Submit(CreateSquadPostModel postModel)
        {
            int? userId = null;
            if (_memberManager.IsLoggedIn())
            {
                userId = int.Parse((await _memberManager.GetCurrentMemberAsync())!.Id);
            }

            var deckId = _deckService.ProcessDeck(postModel, postModel.Publish, userId);
            return Ok(deckId);
        }

        [HttpPost("submitLoggedIn")]
        [ProducesResponseType(typeof(int), 200)]
        [EnableCors("api-login")] //TODO: Rework the authorization correctly
        public async Task<IActionResult> SubmitLoggedIn(CreateSquadPostModel postModel)
        {
            int? userId = null;
            if (_memberManager.IsLoggedIn())
            {
                userId = int.Parse((await _memberManager.GetCurrentMemberAsync())!.Id);
            }

            var deckId = _deckService.ProcessDeck(postModel, postModel.Publish, userId);
            return Ok(deckId);
        }
    }
}
