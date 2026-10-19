using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.Admin.Pricing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;

namespace CardGameDBSites.API.Controllers.Admin
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/admin/pricing")]
    [JwtAuthorization]
    [ApiExplorerSettings(GroupName = "Admin")]
    public class PricingAdminApiController : Controller
    {
        private readonly CardService _cardService;
        private readonly CardPriceService _cardPriceService;
        private readonly CardPriceSyncService _cardPriceSyncService;
        private readonly CollectionService _collectionService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly TcgPlayerIdService _tcgPlayerIdService;

        public PricingAdminApiController(
            CardService cardService,
            CardPriceService cardPriceService,
            CardPriceSyncService cardPriceSyncService,
            CollectionService collectionService,
            ISiteAccessor siteAccessor,
            TcgPlayerIdService tcgPlayerIdService)
        {
            _cardService = cardService;
            _cardPriceService = cardPriceService;
            _cardPriceSyncService = cardPriceSyncService;
            _collectionService = collectionService;
            _siteAccessor = siteAccessor;
            _tcgPlayerIdService = tcgPlayerIdService;
        }

        [HttpGet("overview")]
        [ProducesResponseType(typeof(PricingOverviewApiModel), 200)]
        public IActionResult GetOverview()
        {
            if (!IsAdmin()) return Forbid();

            var variantTypeNames = _collectionService.GetVariantTypes().ToDictionary(it => it.Id, it => it.DisplayName);
            var priceableCards = _cardPriceSyncService.GetPriceableCards();

            var pricedVariantIds = _cardPriceService.GetPrices(priceableCards.Select(it => it.BaseId).Distinct().ToArray())
                .SelectMany(it => it.Prices)
                .Where(it => it.VariantId.HasValue)
                .Select(it => it.VariantId!.Value)
                .ToHashSet();

            var missingPrices = priceableCards
                .Where(it => !pricedVariantIds.Contains(it.VariantId))
                .Select(it => new MissingPriceApiModel
                {
                    Card = Map(it, variantTypeNames),
                    Reason = GetMissingPriceReason(it)
                })
                .OrderBy(it => it.Card.SetName)
                .ThenBy(it => it.Card.DisplayName)
                .ToArray();

            // Child variant types (e.g. foils) share their parent's TCGplayer product, so only priceable cards can conflict.
            var conflicts = priceableCards
                .Select(it => (Card: it, TcgPlayerId: CardPriceSyncService.GetTcgPlayerId(it)))
                .Where(it => !string.IsNullOrEmpty(it.TcgPlayerId))
                .GroupBy(it => it.TcgPlayerId!)
                .Where(it => it.Count() > 1)
                .Select(it => new TcgPlayerIdConflictApiModel
                {
                    TcgPlayerId = it.Key,
                    Cards = it.Select(c => Map(c.Card, variantTypeNames)).ToArray()
                })
                .OrderBy(it => it.TcgPlayerId)
                .ToArray();

            return Ok(new PricingOverviewApiModel
            {
                PriceableCardCount = priceableCards.Length,
                MissingPrices = missingPrices,
                Conflicts = conflicts,
                SyncStatus = MapStatus(_cardPriceSyncService.GetStatus())
            });
        }

        [HttpGet("sync")]
        [ProducesResponseType(typeof(PriceSyncStatusApiModel), 200)]
        public IActionResult GetSyncStatus()
        {
            if (!IsAdmin()) return Forbid();

            return Ok(MapStatus(_cardPriceSyncService.GetStatus()));
        }

        [HttpPost("sync")]
        [ProducesResponseType(typeof(PriceSyncStatusApiModel), 200)]
        [ProducesResponseType(409)]
        public IActionResult StartSync()
        {
            if (!IsAdmin()) return Forbid();

            if (!_cardPriceSyncService.TryStartSiteSync(_siteAccessor.GetSiteId()))
                return Conflict("A price sync is already running.");

            return Ok(MapStatus(_cardPriceSyncService.GetStatus()));
        }

        [HttpPost("tcgplayerid")]
        [ProducesResponseType(typeof(PricingCardApiModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public IActionResult SetTcgPlayerId([FromBody] SetTcgPlayerIdRequest request)
        {
            if (!IsAdmin()) return Forbid();

            var tcgPlayerId = request.TcgPlayerId?.Trim();
            if (!string.IsNullOrEmpty(tcgPlayerId) && !int.TryParse(tcgPlayerId, out _))
                return BadRequest("A TCGplayer ID must be a number.");

            var card = _cardService.GetVariant(request.VariantId);
            if (card is null || card.VariantId == 0) return NotFound();

            var variantTypeNames = _collectionService.GetVariantTypes().ToDictionary(it => it.Id, it => it.DisplayName);
            if (!string.IsNullOrEmpty(tcgPlayerId))
            {
                var existing = _cardPriceSyncService.GetPriceableCards()
                    .FirstOrDefault(it => it.VariantId != card.VariantId && CardPriceSyncService.GetTcgPlayerId(it) == tcgPlayerId);
                if (existing is not null)
                {
                    var other = Map(existing, variantTypeNames);
                    return Conflict($"{tcgPlayerId} is already used by {other.DisplayName} ({other.SetName}, {other.VariantTypeName}).");
                }
            }

            if (!_tcgPlayerIdService.TrySetTcgPlayerId(card.VariantId, tcgPlayerId)) return NotFound();

            var updated = Map(card, variantTypeNames);
            updated.TcgPlayerId = string.IsNullOrEmpty(tcgPlayerId) ? null : tcgPlayerId;
            return Ok(updated);
        }

        private bool IsAdmin() => HttpContext.User.FindFirst("isAdmin")?.Value == "true";

        private static MissingPriceReason GetMissingPriceReason(Card card)
        {
            var tcgPlayerId = CardPriceSyncService.GetTcgPlayerId(card);
            if (string.IsNullOrEmpty(tcgPlayerId)) return MissingPriceReason.NoTcgPlayerId;
            if (!int.TryParse(tcgPlayerId, out _)) return MissingPriceReason.InvalidTcgPlayerId;
            return MissingPriceReason.NoPriceFound;
        }

        private static PricingCardApiModel Map(Card card, Dictionary<int, string> variantTypeNames)
        {
            return new PricingCardApiModel
            {
                BaseId = card.BaseId,
                VariantId = card.VariantId,
                DisplayName = card.DisplayName,
                SetId = card.SetId,
                SetName = card.SetName,
                VariantTypeName = card.VariantTypeId.HasValue && variantTypeNames.TryGetValue(card.VariantTypeId.Value, out var name) ? name : "Base",
                TcgPlayerId = CardPriceSyncService.GetTcgPlayerId(card)
            };
        }

        private PriceSyncStatusApiModel MapStatus(CardPriceSyncStatus status)
        {
            return new PriceSyncStatusApiModel
            {
                IsRunning = status.IsRunning,
                StartedUtc = status.StartedUtc,
                FinishedUtc = status.FinishedUtc,
                SyncedCards = status.SyncedCards,
                Error = status.Error,
                LatestPriceDate = _cardPriceService.GetLatestPriceDate()
            };
        }
    }
}
