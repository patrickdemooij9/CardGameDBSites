using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using System.Text.Encodings.Web;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class CardPriceService
    {
        private readonly CardRepository _cardRepository;
        private readonly CardPriceRepository _cardPriceRepository;
        private readonly IProfiler _profiler;

        public CardPriceService(CardRepository cardRepository, CardPriceRepository cardPriceRepository, IProfiler profiler)
        {
            _cardRepository = cardRepository;
            _cardPriceRepository = cardPriceRepository;
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
                var baseVariantId = _cardRepository.GetVariants(priceGroup.CardId).FirstOrDefault(it => it.VariantTypeId is null);
                if (baseVariantId is null) continue;

                var basePrice = priceGroup.Prices.FirstOrDefault(p => p.VariantId == baseVariantId.VariantId);
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
            var cardPrices = GetBaseCardPrices(deck.Cards.Select(it => it.CardId).ToArray());
            return deck.Cards.Sum(it => !cardPrices.ContainsKey(it.CardId) ? 0 : cardPrices[it.CardId].GetLowest() * it.Amount);
        }

        public string GetUrl(CardPrice cardPrice, Card card)
        {
            //TODO: This should take source id into account.
            const string baseUrl = "https://tcgplayer.pxf.io/c/4924415/1780961/21018";

            var externalId = card.GetMultipleCardAttributeValue("TcgPlayerId")?.FirstOrDefault();
            if (externalId is null) return "";

            return $"{baseUrl}?u={UrlEncoder.Default.Encode($"https://tcgplayer.com/product/{externalId}")}";
        }
    }
}
