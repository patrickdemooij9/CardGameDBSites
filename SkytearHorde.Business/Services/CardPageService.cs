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
            return allCards.FirstOrDefault(it => it.UrlSegment.Equals(urlSegment, StringComparison.InvariantCultureIgnoreCase));
        }

        private CardOverview GetOverview()
        {
            return _siteService.GetRoot().FirstChild<CardOverview>();
        }
    }
}
