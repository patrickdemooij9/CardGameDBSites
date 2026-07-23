using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using System.Diagnostics;
using System.Text.Encodings.Web;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class CardPriceService
    {
        private readonly CardRepository _cardRepository;
        private readonly CardPriceRepository _cardPriceRepository;
        private readonly SetPriceRepository _setPriceRepository;
        private readonly IAppPolicyCache _appPolicyCache;
        private readonly IProfiler _profiler;

        public CardPriceService(CardRepository cardRepository,
            CardPriceRepository cardPriceRepository,
            SetPriceRepository setPriceRepository,
            AppCaches appCaches,
            IProfiler profiler)
        {
            _cardRepository = cardRepository;
            _cardPriceRepository = cardPriceRepository;
            _setPriceRepository = setPriceRepository;
            _appPolicyCache = appCaches.RuntimeCache;
            _profiler = profiler;
        }

        public CardPriceGroup[] GetPrices(params int[] ids)
        {
            return _cardPriceRepository.GetPrices(ids);
        }

        public Dictionary<int, CardPriceGroup> GetBaseCardPrices(params int[] ids)
        {
            var result = new Dictionary<int, CardPriceGroup>();
            foreach (var priceGroup in GetPrices(ids))
            {
                var baseVariants = _cardRepository.GetBase(priceGroup.CardId).ToArray();
                foreach (var baseVariant in baseVariants)
                {
                    var basePrice = priceGroup.Prices.FirstOrDefault(p => p.VariantId == baseVariant.VariantId);
                    if (basePrice is null) continue;

                    if (result.TryGetValue(priceGroup.CardId, out CardPriceGroup? value))
                    {
                        value.Prices.Add(basePrice);
                        continue;
                    }

                    var basePriceGroup = new CardPriceGroup();
                    basePriceGroup.Prices.Add(basePrice);
                    result.Add(priceGroup.CardId, basePriceGroup);
                }
            }
            return result;
        }

        public Dictionary<int, CardPrice> GetPrices(params Card[] cards)
        {
            return GetPrices(cards.Select(it => it.BaseId).Distinct().ToArray())
                .SelectMany(it => it.Prices)
                .GroupBy(it => it.VariantId)
                .ToDictionary(it => it.Key!.Value, it => it.First());
        }

        public double GetPriceByDeck(Deck deck)
        {
            return _appPolicyCache.GetCacheItem($"CardPriceService_PriceByDeck_{deck.Id}", () =>
            {
                using var _ = _profiler.Step("GetPriceByDeck");
                var cardPrices = GetBaseCardPrices([.. deck.Cards.Select(it => it.CardId)]);
                return deck.Cards.Sum(it => !cardPrices.TryGetValue(it.CardId, out CardPriceGroup? value) ? 0 : value.GetLowest() * it.Amount);
            }, TimeSpan.FromMinutes(5));
        }

        public List<CardPriceChangeResult> GetTopPriceChanges(int count, bool descending, int? variantTypeId = null)
        {
            var allChanges = _cardPriceRepository.GetPriceChanges(descending);
            var cards = _cardRepository.Get([.. allChanges.Distinct().Select(it => it.VariantId!.Value)]).ToDictionary(it => it.VariantId, it => it);
            return allChanges.Where(it => cards.TryGetValue(it.VariantId!.Value, out var variant) && variant.VariantTypeId == variantTypeId).Take(count).ToList();
        }
        public DateTime? GetLatestPriceDate() => _cardPriceRepository.GetLatestPriceDate();

        public List<CardPriceChangeResult> GetTopWeeklyPriceChanges(int count, bool descending, int? variantTypeId = null)
        {
            var allChanges = _cardPriceRepository.GetWeeklyPriceChanges(descending);
            var cards = _cardRepository.Get([.. allChanges.Distinct().Select(it => it.VariantId!.Value)]).ToDictionary(it => it.VariantId, it => it);
            return allChanges.Where(it => cards.TryGetValue(it.VariantId!.Value, out var variant) && variant.VariantTypeId == variantTypeId).Take(count).ToList();
        }

        public List<CardPrice> GetPriceHistory(int cardId, int? variantId)
        {
            return _cardPriceRepository.GetPriceHistory(cardId, variantId)
                .Select(r => new CardPrice
                {
                    VariantId = r.VariantId,
                    MainPrice = r.MainPrice,
                    LowestPrice = r.LowestPrice,
                    HighestPrice = r.HighestPrice,
                    DateUtc = r.DateUtc,
                })
                .ToList();
        }

        public string GetUrl(CardPrice cardPrice, Card card)
        {
            //TODO: This should take source id into account.
            const string baseUrl = "https://tcgplayer.pxf.io/c/4924415/1780961/21018";

            var externalId = card.GetMultipleCardAttributeValue("TcgPlayerId")?.FirstOrDefault();
            if (externalId is null) return "";

            return $"{baseUrl}?u={UrlEncoder.Default.Encode($"https://tcgplayer.com/product/{externalId}")}";
        }

        public double? GetSetPrice(int setId)
        {
            return _setPriceRepository.GetLatestPrice(setId)?.TotalPrice;
        }

        public List<SetPricePoint> GetSetPriceHistory(int setId)
        {
            return _setPriceRepository.GetPriceHistory(setId)
                .Select(r => new SetPricePoint
                {
                    TotalPrice = r.TotalPrice,
                    DateUtc = r.DateUtc,
                })
                .ToList();
        }
    }
}
