using SkytearHorde.Business.Services;

namespace SkytearHorde.Business.Facts.Generators
{
    public class LeastExpensiveCardGenerator : FactGeneratorBase
    {
        private readonly CardPriceService _cardPriceService;
        private readonly SettingsService _settingsService;

        public LeastExpensiveCardGenerator(CardPriceService cardPriceService, SettingsService settingsService)
        {
            _cardPriceService = cardPriceService;
            _settingsService = settingsService;
        }

        public override string Key => "least-expensive-card";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            if (!_settingsService.GetSiteSettings().AllowPricing) return null;

            var prices = _cardPriceService.GetPrices([.. context.Cards]);

            var bottom = context.Cards
                .Select(card => (card, price: prices.TryGetValue(card.VariantId, out var p) ? p.MainPrice : 0))
                .Where(x => x.price > 0)
                .OrderBy(x => x.price)
                .ThenBy(x => x.card.DisplayName)
                .FirstOrDefault();

            if (bottom.card is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Do you know which card is the cheapest in {ScopeLabel(context)}?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "CHEAPEST CARD",
                        Title = bottom.card.DisplayName,
                        BigValue = Money(bottom.price),
                        BigLabel = "MARKET PRICE",
                        Caption = bottom.card.SetName,
                        ImageUrl = ImageRel(bottom.card)
                    }
                ]
            };
        }
    }
}
