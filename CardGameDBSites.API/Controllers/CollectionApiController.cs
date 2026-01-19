using CardGameDBSites.API.Models.Collection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using YamlDotNet.Core.Tokens;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/collection")]
    [Authorize(AuthenticationSchemes = "Jwt")]
    public class CollectionApiController : Controller
    {
        private readonly CollectionService _collectionService;
        private readonly CardPriceService _cardPriceService;
        private readonly CardService _cardService;

        public CollectionApiController(CollectionService collectionService,
            CardPriceService cardPriceService,
            CardService cardService)
        {
            _collectionService = collectionService;
            _cardPriceService = cardPriceService;
            _cardService = cardService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(CollectionSummaryApiModel), 200)]
        public IActionResult GetSummary()
        {
            var cards = _collectionService.GetCards();
            var prices = _cardPriceService.GetPrices(cards.Select(it => it.CardId).Distinct().ToArray());

            var marketPrice = 0d;
            foreach (var card in cards)
            {
                var cardPrices = prices.FirstOrDefault(it => it.CardId == card.CardId);
                if (cardPrices is null) continue;

                var cardPrice = cardPrices.Prices.FirstOrDefault(it => it.VariantId == card.VariantId);
                if (cardPrice is null) continue;

                marketPrice += cardPrice.MainPrice * card.Amount;
            }

            return Ok(new CollectionSummaryApiModel
            {
                PacksOpened = _collectionService.GetPackCount(),
                UniqueCards = cards.Select(it => it.CardId).Distinct().Count(),
                TotalCards = cards.Sum(it => it.Amount),
                MarketPrice = marketPrice
            });
        }

        [HttpGet("setsProgress")]
        [ProducesResponseType(typeof(SetProgressApiModel[]), 200)]
        public IActionResult GetSetsProgress()
        {
            var result = new List<SetProgressApiModel>();
            var sets = _cardService.GetAllSets();
            foreach (var set in sets)
            {
                _collectionService.CalculateCollectionProgressBySet(set.Id, out var totalCards, out var collectionCards);

                result.Add(new SetProgressApiModel
                {
                    SetId = set.Id,
                    TotalCards = totalCards,
                    OwnedCards = collectionCards
                });
            }
            return Ok(result);
        }

        [HttpPost("cards")]
        [ProducesResponseType(typeof(CollectionCardApiModel[]), 200)]
        public IActionResult GetCollectionCards(int[] cardIds)
        {
            return Ok(_collectionService.GetCards()
                .Where(it => cardIds.Contains(it.CardId))
                .Select(it => new CollectionCardApiModel
                {
                    Id = it.Id,
                    CardId = it.CardId,
                    VariantId = it.VariantId,
                    Amount = it.Amount
                }));
        }

        [HttpPost("addCards")]
        [ProducesResponseType(typeof(CollectionCardApiModel[]), 200)]
        public IActionResult AddCollectionCards(int cardId, Dictionary<int, int> values)
        {
            var card = _cardService.Get(cardId);
            if (card is null) return NotFound();

            _collectionService.UpdateCard(cardId, values);

            //TODO: Rework this because this will probably be slow
            return Ok(_collectionService.GetCards()
                .Where(it => values.Keys.Contains(it.VariantId))
                .Select(it => new CollectionCardApiModel
                {
                    Id = it.Id,
                    CardId = it.CardId,
                    VariantId = it.VariantId,
                    Amount = it.Amount
                }));
        }
    }
}
