using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Cards;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RedditSharp;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/cards")]
    public class CardApiController : Controller
    {
        private readonly ICardSearchService _cardSearchService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CardApiController(ICardSearchService cardSearchService,
            ISiteAccessor siteAccessor,
            CardService cardService,
            SettingsService settingsService,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _cardSearchService = cardSearchService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _settingsService = settingsService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("all")]
        public IActionResult GetAll(int skip, int take)
        {
            var result = _cardSearchService.Search(new CardSearchQuery(take, _siteAccessor.GetSiteId())
            {
                Skip = skip
            }, out _);
            return Ok(result);
        }

        [HttpPost("byIds")]
        [ProducesResponseType(typeof(CardDetailApiModel[]), 200)]
        public IActionResult ByIds(int[] ids)
        {
            var cards = _cardService.Get(ids);
            return Ok(cards.Select(it => new CardDetailApiModel(it)));
        }

        [HttpGet("byId")]
        [ProducesResponseType(typeof(CardDetailApiModel), 200)]
        public IActionResult ById(Guid id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoCard = ctx.UmbracoContext.Content.GetById(id);
            if (umbracoCard is null) return NotFound();

            return Ok(new CardDetailApiModel(_cardService.Get(umbracoCard.Id)));
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(PagedResult<CardDetailApiModel>), 200)]
        public IActionResult Query(CardsQueryPostApiModel model)
        {
            var sorting = new List<CardSorting>();
            var defaultSortOptions = _settingsService.GetSiteSettings().SortOptions;
            if (defaultSortOptions.All(it => it.Values?.Any() != true))
            {
                sorting.AddRange(defaultSortOptions.Select(it => new CardSorting(it.ExamineField.IfNullOrWhiteSpace($"CustomField.{it.Ability?.Name}")) { IsDescending = it.Descending }));
            }

            var result = _cardSearchService.Search(new CardSearchQuery(model.PageSize, _siteAccessor.GetSiteId())
            {
                Query = model.Query,
                Skip = model.PageSize * (model.PageNumber - 1),
                SetId = model.SetId,
                FilterClauses = [.. model.FilterClauses.Select(it => new CardSearchFilterClause
                {
                    ClauseType = it.ClauseType,
                    Filters = [.. it.Filters.Select(f => new CardSearchFilter
                    {
                        Alias = f.Alias,
                        Values = f.Values,
                        Mode = f.Mode,
                    })]
                })],
                OrderBy = sorting,
                VariantTypeId = model.VariantTypeId
            }, out var totalItems).Select(it => new CardDetailApiModel(it));
            return Ok(new PagedResult<CardDetailApiModel>(totalItems, model.PageNumber, model.PageSize)
            {
                Items = result
            });
        }

        [HttpGet("getAllValues")]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetAllValues(string abilityName)
        {
            return Ok(_cardService.GetCardValues(abilityName));
        }
    }
}
