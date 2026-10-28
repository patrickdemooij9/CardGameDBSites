using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class CardFilterLandingService
    {
        public const string RequestCacheKey = "cardFilterLanding";

        private readonly ISiteService _siteService;
        private readonly CardService _cardService;
        private readonly IShortStringHelper _shortStringHelper;

        public CardFilterLandingService(ISiteService siteService, CardService cardService, IShortStringHelper shortStringHelper)
        {
            _siteService = siteService;
            _cardService = cardService;
            _shortStringHelper = shortStringHelper;
        }

        public CardFilterLandingCollection[] GetCollections()
        {
            return [.. (_siteService.GetCardOverview().Children<CardFilterCollection>() ?? [])
                .Where(it => it.CardAttribute is not null)
                .Select(ToCollection)];
        }

        public (CardFilterLandingCollection Collection, CardFilterLandingItem Landing)? FindGenerated(string path)
        {
            path = path.TrimEnd('/');
            var candidates = (_siteService.GetCardOverview().Children<CardFilterCollection>() ?? [])
                .Where(it => it.AutoGenerateLandings && it.CardAttribute is not null
                    && path.StartsWith(it.Url(mode: UrlMode.Relative), StringComparison.InvariantCultureIgnoreCase));
            foreach (var collection in candidates.Select(ToCollection))
            {
                var landing = collection.Landings.FirstOrDefault(it =>
                    it.IsGenerated && it.Url.Equals(path, StringComparison.InvariantCultureIgnoreCase));
                if (landing is not null) return (collection, landing);
            }
            return null;
        }

        private CardFilterLandingCollection ToCollection(CardFilterCollection collection)
        {
            var attribute = collection.CardAttribute!.Name;
            var url = collection.Url(mode: UrlMode.Relative).TrimEnd('/');

            var cmsLandings = (collection.Children<Entities.Generated.CardFilterLanding>() ?? [])
                .Where(it => !string.IsNullOrWhiteSpace(it.FilterValue))
                .Select(landing => new CardFilterLandingItem
                {
                    Name = landing.Name,
                    Url = landing.Url(mode: UrlMode.Relative).TrimEnd('/'),
                    Value = landing.FilterValue!,
                    CardCount = _cardService.CountByValue(attribute, landing.FilterValue!)
                })
                .ToArray();

            if (!collection.AutoGenerateLandings)
            {
                return new CardFilterLandingCollection { Content = collection, Url = url, Attribute = attribute, Landings = cmsLandings };
            }

            var cmsValues = cmsLandings.Select(it => it.Value).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var generated = _cardService.GetCardValues(attribute)
                .Where(value => !string.IsNullOrWhiteSpace(value) && !cmsValues.Contains(value))
                .Select(value => new CardFilterLandingItem
                {
                    Name = value,
                    Url = $"{url}/{value.ToUrlSegment(_shortStringHelper)}",
                    Value = value,
                    CardCount = _cardService.CountByValue(attribute, value),
                    IsGenerated = true
                })
                .Where(it => it.CardCount > 0);

            return new CardFilterLandingCollection
            {
                Content = collection,
                Url = url,
                Attribute = attribute,
                Landings = [.. cmsLandings.Concat(generated).OrderBy(it => it.Name, StringComparer.InvariantCultureIgnoreCase)]
            };
        }
    }
}
