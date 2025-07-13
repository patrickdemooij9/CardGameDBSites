using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    public class CardService
    {
        private readonly ISiteAccessor _siteAccessor;
        private readonly ICardSearchService _cardSearchService;
        private readonly CardRepository _cardRepository;
        private readonly IAppCache _cache;

        public CardService(AppCaches appCaches, ISiteAccessor siteAccessor, ICardSearchService cardSearchService, CardRepository cardRepository)
        {
            _siteAccessor = siteAccessor;
            _cardSearchService = cardSearchService;
            _cardRepository = cardRepository;
            _cache = appCaches.RuntimeCache;
        }

        public Card? Get(int id)
        {
            return _cardRepository.Get(id);
        }

        public Card? GetVariant(int id)
        {
            return _cardRepository.GetVariant(id);
        }

        public IEnumerable<Card> Get(int[] ids)
        {
            return _cardRepository.Get(ids);
        }

        public IEnumerable<Card> GetVariantsForVariant(int id)
        {
            return _cardRepository.GetVariantsForVariant(id);
        }

        public IEnumerable<Card> GetVariants(int id)
        {
            return _cardRepository.GetVariants(id);
        }

        public IEnumerable<Card> GetAll(bool includeVariants = false)
        {
            return _cardRepository.GetAll(includeVariants);
        }

        public IEnumerable<Card> GetAllBySetCode(string setCode, bool includeVariants = false)
        {
            return _cardRepository.GetAllBySetCode(setCode, includeVariants);
        }

        public IEnumerable<Card> GetAllBySet(int setId, bool includeVariants = false)
        {
            var cards = Search(new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId())
            {
                SetId = setId,
            }, out var _);
            if (includeVariants)
            {
                return cards;
            }
            return cards.Where(it => it.VariantTypeId is null).ToArray();
        }

        public IEnumerable<Card> GetAllBaseBySet(int setId)
        {
            return GetAllBySet(setId, true).Where(it => it.VariantTypeId is null && it.VariantId > 0);
        }

        public IEnumerable<Set> GetAllSets()
        {
            return _cardRepository.GetAllSets();
        }

        public IEnumerable<string> GetCardValues(string key)
        {
            return _cache.GetCacheItem($"GetCardValues_{_siteAccessor.GetSiteId()}_{key}", () =>
            {
                return GetAll().SelectMany(it => it.GetMultipleCardAttributeValue(key) ?? Enumerable.Empty<string>()).Distinct().ToArray();
            })!.ToArray();
        }

        public Card[] Search(CardSearchQuery query, out int totalItems)
        {
            var ids = _cardSearchService.Search(query, out totalItems);
            return [.. ids.Select(GetVariant).Where(card => card != null).Cast<Card>()];
        }

        public void ClearCache()
        {
            _cache.ClearByKey("GetCardValues_");
        }
    }
}
