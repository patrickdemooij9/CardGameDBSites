using CardGameDBSites.API.Models.Sets;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
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
        private readonly ISiteService _siteService;

        public SetsApiController(CardService cardService, ISiteService siteService)
        {
            _cardService = cardService;
            _siteService = siteService;
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

        private SetViewModel CreateSetViewModel(Set set)
        {
            var setOverviewPageUrl = $"/{_siteService.GetSetOverview()?.UrlSegment()}";
            return new SetViewModel
            {
                Id = set.Id,
                DisplayName = set.DisplayName!,
                UrlSegment = $"{setOverviewPageUrl}/{set.UrlSegment()}",
                ImageUrl = set.DisplayImage?.Url(mode: UrlMode.Absolute),
                Code = set.SetCode,
                Category = set.CategoryName,
                ExtraInformation = set.ExtraInformation?.ToArray() ?? []
            };
        }
    }
}
