using CardGameDBSites.API.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.CustomCardMaker;
using SkytearHorde.Entities.Models.PostModels;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/cardmaker")]
    public class CardMakerApiController : Controller
    {
        private readonly CardMaker _cardMaker;

        public CardMakerApiController(CardMaker cardMaker)
        {
            _cardMaker = cardMaker;
        }

        [HttpPost("render")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Render([FromBody] RenderCardPostModel model)
        {
            var bytes = await _cardMaker.Generate(model);
            return Ok(Convert.ToBase64String(bytes));
        }

        [HttpPost("download")]
        public async Task<IActionResult> Download([FromBody] RenderCardPostModel model)
        {
            var bytes = await _cardMaker.Generate(model);
            return File(bytes, "image/png", "YourCard.png");
        }
    }
}
