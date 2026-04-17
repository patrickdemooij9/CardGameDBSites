using CardGameDBSites.API.Models.Sets;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Models;
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
            return Ok(_cardService.GetAllSets().Select(CreateSetViewModel));
        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(SetViewModel), 200)]
        public IActionResult GetSet(int id)
        {
            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Id == id);
            if (set is null) return NotFound();
            return Ok(CreateSetViewModel(set));
        }

        [HttpGet("getByKey")]
        [ProducesResponseType(typeof(SetViewModel), 200)]
        public IActionResult GetSet(Guid key)
        {
            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Key == key);
            if (set is null) return NotFound();
            return Ok(CreateSetViewModel(set));
        }

        [HttpGet("getByIds")]
        [ProducesResponseType(typeof(SetViewModel[]), 200)]
        public IActionResult GetSetsByIds(int[] ids)
        {
            return Ok(_cardService.GetAllSets().Where(it => ids.Contains(it.Id)).Select(CreateSetViewModel));
        }

        private static SetViewModel CreateSetViewModel(IPublishedContent set)
        {
            // TODO: After adding the category property in Umbraco models, switch this to the strongly typed Set.Category property.
            var category = set.Value<string>("category");
            if (string.IsNullOrWhiteSpace(category))
            {
                category = null;
            }

            return new SetViewModel
            {
                Id = set.Id,
                DisplayName = set.Value<string>("displayName") ?? set.Name,
                UrlSegment = set.UrlSegment()!,
                ImageUrl = set.Value<MediaWithCrops>("displayImage")?.Url(mode: UrlMode.Absolute),
                Code = set.Value<string>("setCode"),
                Category = category,
                ExtraInformation = set.Value<IEnumerable<string>>("extraInformation")?.ToArray() ?? []
            };
        }
    }
}
