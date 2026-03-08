using CardGameDBSites.API.Models.Collection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Exports.Collection;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
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
        private readonly SettingsService _settingsService;

        public CollectionApiController(CollectionService collectionService,
            CardPriceService cardPriceService,
            CardService cardService,
            SettingsService settingsService)
        {
            _collectionService = collectionService;
            _cardPriceService = cardPriceService;
            _cardService = cardService;
            _settingsService = settingsService;
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

        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public async Task<IActionResult> ExportCollection(CollectionExportType exportType)
        {
            var collection = _collectionService.GetCards();
            var variantTypes = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();

            byte[] data;
            if (exportType == CollectionExportType.Grouped)
            {
                data = await new ExcelCollectionManager(variantTypes, _cardService.GetAllSets().ToArray(), _cardService).Export(collection);
            }
            else
            {
                data = await new DetailedCollectionImport(variantTypes, _cardService, _settingsService.GetCollectionSettings().ImportMappings).Export(collection);
            }

            return File(data, "application/octet-stream", "CollectionExport.xlsx");
        }

        [HttpPost("import")]
        [ProducesResponseType(typeof(IActionResult), 200)]
        public async Task<IActionResult> ImportCollection(bool overwrite)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file is null) return NotFound();

            var variants = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();
            var items = new List<CollectionCardItem>();

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".csv"))
                    {
                        items.AddRange(new CsvCollectionManager(variants, _cardService, _settingsService.GetCollectionSettings().ImportMappings).Import(stream));
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
                    {
                        items.AddRange(new ExcelCollectionManager(variants, _cardService.GetAllSets().ToArray(), _cardService).Import(stream));
                    }
                    else
                    {
                        return BadRequest("File should be an .xlsx or .csv (SWUDB format)");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (items.Count == 0)
            {
                return BadRequest("No rows found to import");
            }

            var collection = _collectionService.GetCards();
            if (overwrite)
            {
                _collectionService.ClearCollection();
            }
            try
            {
                foreach (var item in items)
                {
                    _collectionService.UpdateCard(item.CardId, item.VariantId, item.Amount);
                }
            }
            catch
            {
                if (overwrite)
                {
                    foreach (var item in collection)
                    {
                        _collectionService.UpdateCard(item.CardId, item.VariantId, item.Amount);
                    }
                }
            }

            return Ok();
        }

        [HttpPost("verifyPack")]
        [ProducesResponseType(typeof(PackVerifySuccessApiModel), 200)]
        [ProducesResponseType(typeof(PackVerifyErrorApiModel), 400)]
        public IActionResult VerifyPack([FromBody] PackPostApiModel postModel)
        {
            var sets = _cardService.GetAllSets().ToArray();
            var set = sets.FirstOrDefault(it => it.Id == postModel.SetId);
            if (set is null) return NotFound();

            var items = postModel.Items.Select(it => new PackItemPostModel
            {
                Id = it.Id,
                VariantTypeId = it.VariantTypeId
            }).ToArray();

            var cards = _collectionService.ValidateCardsInPack(set.Id, items, out var invalidItems);

            if (invalidItems.Count > 0)
            {
                return BadRequest(new PackVerifyErrorApiModel
                {
                    ErrorMessage = $"Could not resolve items with ID: {string.Join(',', invalidItems.Select(it => it.Id))}",
                    PostContent = postModel
                });
            }

            return Ok(new PackVerifySuccessApiModel
            {
                SetId = set.Id,
                Cards = cards.Select(it => new PackVerifyCardApiModel
                {
                    BaseId = it.BaseId,
                    DisplayName = it.DisplayName,
                    VariantTypeId = it.VariantTypeId
                }).ToArray()
            });
        }

        [HttpPost("addPack")]
        [ProducesResponseType(200)]
        public IActionResult AddPack([FromBody] PackPostApiModel postModel)
        {
            var items = postModel.Items.Select(it => new PackItemPostModel
            {
                Id = it.Id,
                VariantTypeId = it.VariantTypeId
            }).ToArray();

            _collectionService.AddPack(postModel.SetId, items);

            return Ok();
        }
    }
}
