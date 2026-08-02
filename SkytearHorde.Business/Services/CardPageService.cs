using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    public class CardPageService
    {
        private readonly CardService _cardService;
        private readonly ISiteService _siteService;

        public CardPageService(CardService cardService, ISiteService siteService)
        {
            _cardService = cardService;
            _siteService = siteService;
        }

        public string GetUrl(Card card)
        {
            var parentSet = _cardService.GetAllSets().FirstOrDefault(it => it.Id == card.SetId);
            return GetUrl(card, parentSet);
        }

        public string GetUrl(Card card, Set? set)
        {
            var overview = GetOverview();
            if (!string.IsNullOrWhiteSpace(set?.SetCode))
            {
                return $"{overview.Url(mode: UrlMode.Relative)}{set!.SetCode}/{card.UrlSegment}";
            }
            return $"{overview.Url(mode: UrlMode.Relative)}{card.UrlSegment}";
        }

        public Card? GetByUrl(string urlSegment, string? setCode = null)
        {
            var allCards = string.IsNullOrWhiteSpace(setCode) ? _cardService.GetAll(true) : _cardService.GetAllBySetCode(setCode, true);
            return allCards.FirstOrDefault(it => it.VariantId > 0 && it.UrlSegment.Equals(urlSegment, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Resolves a card from the URL path segments that follow an overview page (e.g. the parts after
        /// "/cards/"). Handles an optional leading set-code segment, mirroring how card page URLs are built.
        /// </summary>
        public Card? GetBySegments(string[] segments)
        {
            if (segments.Length == 0) return null;

            var sets = _cardService.GetAllSets().Where(it => !string.IsNullOrWhiteSpace(it.SetCode));
            var potentialSet = sets.FirstOrDefault(it => it.SetCode?.Equals(segments[0], StringComparison.InvariantCultureIgnoreCase) is true);

            string urlSegment;
            if (potentialSet is null)
            {
                urlSegment = segments.Length == 1 ? segments[0] : $"{segments[0]}/{segments[1]}";
            }
            else
            {
                urlSegment = segments.Length == 2 ? segments[1] : $"{segments[1]}/{segments[2]}";
            }

            return GetByUrl(urlSegment, potentialSet?.SetCode);
        }

        private CardOverview GetOverview()
        {
            return _siteService.GetRoot().FirstChild<CardOverview>();
        }
    }
}
