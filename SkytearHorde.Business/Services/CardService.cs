using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
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
        private readonly CardRepository _cardRepository;
        private readonly IAppCache _cache;

        public CardService(AppCaches appCaches, ISiteAccessor siteAccessor, CardRepository cardRepository)
        {
            _siteAccessor = siteAccessor;
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
            return _cardRepository.GetAllBySet(setId, includeVariants);
        }

        public IEnumerable<Card> GetAllBaseBySet(int setId)
        {
            return _cardRepository.GetAllBySet(setId, true).Where(it => it.VariantTypeId is null && it.VariantId > 0);
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

        public void ClearCache()
        {
            _cache.ClearByKey("GetCardValues_");
        }
    }
}
