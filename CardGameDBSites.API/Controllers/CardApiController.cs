using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Cards;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/cards")]
    public class CardApiController : Controller
    {
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly CollectionService _collectionService;
        private readonly CardPriceService _cardPriceService;
        private readonly SettingsService _settingsService;
        private readonly ISiteService _siteService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IMemberManager _memberManager;
        private readonly CardPageService _cardPageService;

        public CardApiController(ISiteAccessor siteAccessor,
            CardService cardService,
            CollectionService collectionService,
            CardPriceService cardPriceService,
            SettingsService settingsService,
            ISiteService siteService,
            IUmbracoContextFactory umbracoContextFactory,
            IMemberManager memberManager,
            CardPageService cardPageService)
        {
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _collectionService = collectionService;
            _cardPriceService = cardPriceService;
            _settingsService = settingsService;
            _siteService = siteService;
            _umbracoContextFactory = umbracoContextFactory;
            _memberManager = memberManager;
            _cardPageService = cardPageService;
        }

        [HttpGet("all")]
        public IActionResult GetAll(int skip, int take)
        {
            var result = _cardService.Search(new CardSearchQuery(take, _siteAccessor.GetSiteId())
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
            return Ok(cards.Select(MapToApiModel));
        }

        [HttpGet("byId")]
        [ProducesResponseType(typeof(CardDetailApiModel), 200)]
        public IActionResult ById(Guid id, int? setId = null)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoCard = ctx.UmbracoContext.Content.GetById(id);
            if (umbracoCard is null) return NotFound();

            var baseVariant = _cardService.GetBaseVariants(umbracoCard.Id).FirstOrDefault(it => setId is null || it.SetId == setId);
            if (baseVariant is null) return NotFound();

            return Ok(MapToApiModel(baseVariant));
        }

        [HttpPost("query")]
        [OptionalJwtAuthorization]
        [ProducesResponseType(typeof(PagedResult<CardDetailApiModel>), 200)]
        public IActionResult Query(CardsQueryPostApiModel model)
        {
            var sorting = new List<CardSorting>();
            if (!string.IsNullOrWhiteSpace(model.SortBy))
            {
                var cardOverview = _siteService.GetCardOverview();
                var selectedSorting = cardOverview.Sortings.ToItems<SkytearHorde.Entities.Generated.SortingItem>().FirstOrDefault(it => it.Value == model.SortBy);
                if (selectedSorting != null && !string.IsNullOrWhiteSpace(selectedSorting.ExamineField))
                {
                    sorting.Add(new CardSorting(selectedSorting.ExamineField) { IsDescending = selectedSorting.Descending });
                }
            }
            else
            {
                var defaultSortOptions = _settingsService.GetSiteSettings().SortOptions;
                if (defaultSortOptions.All(it => it.Values?.Any() != true))
                {
                    sorting.AddRange(defaultSortOptions.Select(it => new CardSorting(it.ExamineField.IfNullOrWhiteSpace($"CustomField.{it.Ability?.Name}")) { IsDescending = it.Descending }));
                }
            }

            int? memberId = null;
            if (model.OnlyOwnedCards && _memberManager.IsLoggedIn())
            {
                var member = _memberManager.GetCurrentMemberAsync().GetAwaiter().GetResult();
                if (member != null && int.TryParse(member.Id, out var id))
                {
                    memberId = id;
                }
            }

            var result = _cardService.Search(new CardSearchQuery(model.PageSize, _siteAccessor.GetSiteId())
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
                VariantTypeId = model.VariantTypeId,
                OnlyOwnedCards = model.OnlyOwnedCards,
                MemberId = memberId,
                IncludeReprintedCards = model.IncludeReprintedCards,
                LegalForDeckTypeId = model.LegalForDeckTypeId
            }, out var totalItems).Select(MapToApiModel);
            return Ok(new PagedResult<CardDetailApiModel>(totalItems, model.PageNumber, model.PageSize)
            {
                Items = result
            });
        }

        [HttpGet("getAllValues")]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetAllValues(string abilityName)
        {
            return Ok(_cardService.GetCardValues(abilityName).Order());
        }

        [HttpGet("variantTypes")]
        [ProducesResponseType(typeof(CardVariantTypeApiModel[]), 200)]
        public IActionResult GetVariantTypes()
        {
            return Ok(_collectionService.GetVariantTypes().Select(it => new CardVariantTypeApiModel
            {
                Id = it.Id,
                DisplayName = it.DisplayName,
                HasPage = it.HasPage,
                Color = it.Color,
                Initial = it.Initial
            }));
        }

        [HttpGet("topPriceChanges")]
        [ProducesResponseType(typeof(CardPriceChangeApiModel[]), 200)]
        public IActionResult GetTopPriceChanges(int count, bool descending)
        {
            if (!_settingsService.GetSiteSettings().AllowPricing)
                return Ok(Array.Empty<CardPriceChangeApiModel>());

            if (count <= 0 || count > 100)
                return BadRequest("count must be between 1 and 100.");

            var changes = _cardPriceService.GetTopPriceChanges(count, descending);
            var cardIds = changes.Select(c => c.CardId).Distinct().ToArray();
            var cards = _cardService.Get(cardIds).ToDictionary(c => c.BaseId);

            var result = changes
                .Where(c => cards.ContainsKey(c.CardId))
                .Select(c =>
                {
                    var card = cards[c.CardId];
                    var priceChange = c.CurrentPrice - c.PreviousPrice;
                    var priceChangePercent = c.PreviousPrice > 0
                        ? (priceChange / c.PreviousPrice) * 100.0
                        : 0.0;
                    return new CardPriceChangeApiModel
                    {
                        CardId = c.CardId,
                        VariantId = c.VariantId,
                        CardName = card.DisplayName ?? string.Empty,
                        UrlSegment = _cardPageService.GetUrl(card),
                        CurrentPrice = c.CurrentPrice,
                        PreviousPrice = c.PreviousPrice,
                        PriceChange = Math.Round(priceChange, 2),
                        PriceChangePercent = Math.Round(priceChangePercent, 2)
                    };
                })
                .ToArray();

            return Ok(result);
        }

        [HttpGet("priceHistory")]
        [ProducesResponseType(typeof(CardPriceHistoryItemApiModel[]), 200)]
        public IActionResult GetPriceHistory(int cardId, int? variantId)
        {
            if (!_settingsService.GetSiteSettings().AllowPricing)
                return Ok(Array.Empty<CardPriceHistoryItemApiModel>());

            var history = _cardPriceService.GetPriceHistory(cardId, variantId);
            var result = history.Select(h => new CardPriceHistoryItemApiModel
            {
                Date = h.DateUtc.ToString("yyyy-MM-dd"),
                Price = h.MainPrice
            }).ToArray();

            return Ok(result);
        }

        private CardDetailApiModel MapToApiModel(Card card)
        {
            var detail = new CardDetailApiModel(card, card.NonLegalDeckTypes, _cardPageService.GetUrl(card));
            if (_settingsService.GetSiteSettings().AllowPricing)
            {
                var prices = _cardPriceService.GetPrices(card);
                if (prices.TryGetValue(card.VariantId, out CardPrice? value))
                {
                    detail.Price = new CardPriceApiModel
                    {
                        MarketPrice = value.MainPrice,
                        ReferenceUrl = _cardPriceService.GetUrl(value, card)
                    };
                }
            }

            return detail;
        }
    }
}
