using CardGameDBSites.API.Models.Sets;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/sets")]
    public class SetsApiController : Controller
    {
        private readonly CardService _cardService;

        public SetsApiController(CardService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet("getAll")]
        [ProducesResponseType(typeof(SetViewModel[]), 200)]
        public IActionResult GetSets()
        {
            return Ok(_cardService.GetAllSets().Select(it => new SetViewModel
            {
                Id = it.Id,
                DisplayName = it.DisplayName!,
                UrlSegment = it.UrlSegment()!,
                ImageUrl = it.DisplayImage?.Url(mode: UrlMode.Absolute),
                Code = it.SetCode,
                ExtraInformation = it.ExtraInformation?.ToArray() ?? []
            }));
        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(SetViewModel), 200)]
        public IActionResult GetSet(int id)
        {
            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Id == id);
            if (set is null) return NotFound();
            return Ok(new SetViewModel
            {
                Id = id,
                DisplayName = set.DisplayName!,
                UrlSegment = set.UrlSegment()!,
                ImageUrl = set.DisplayImage?.Url(mode: UrlMode.Absolute),
                Code = set.SetCode,
                ExtraInformation = set.ExtraInformation?.ToArray() ?? []
            });
        }

        [HttpGet("getByKey")]
        [ProducesResponseType(typeof(SetViewModel), 200)]
        public IActionResult GetSet(Guid key)
        {
            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Key == key);
            if (set is null) return NotFound();
            return Ok(new SetViewModel
            {
                Id = set.Id,
                DisplayName = set.DisplayName!,
                UrlSegment = set.UrlSegment()!,
                ImageUrl = set.DisplayImage?.Url(mode: UrlMode.Absolute),
                Code = set.SetCode,
                ExtraInformation = set.ExtraInformation?.ToArray() ?? []
            });
        }
    }
}
