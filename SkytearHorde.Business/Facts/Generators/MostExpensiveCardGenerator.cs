using SkytearHorde.Business.Services;

namespace SkytearHorde.Business.Facts.Generators
{
    public class MostExpensiveCardGenerator : FactGeneratorBase
    {
        private readonly CardPriceService _cardPriceService;
        private readonly SettingsService _settingsService;

        public MostExpensiveCardGenerator(CardPriceService cardPriceService, SettingsService settingsService)
        {
            _cardPriceService = cardPriceService;
            _settingsService = settingsService;
        }

        public override string Key => "most-expensive-card";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            if (!_settingsService.GetSiteSettings().AllowPricing) return null;

            var prices = _cardPriceService.GetPrices([.. context.Cards]);

            var top = context.Cards
                .Select(card => (card, price: prices.TryGetValue(card.VariantId, out var p) ? p.MainPrice : 0))
                .Where(x => x.price > 0)
                .OrderByDescending(x => x.price)
                .ThenBy(x => x.card.DisplayName)
                .FirstOrDefault();

            if (top.card is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Do you know which card is the most expensive in {ScopeLabel(context)}?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "MOST EXPENSIVE",
                        Title = top.card.DisplayName,
                        BigValue = Money(top.price),
                        BigLabel = "MARKET PRICE",
                        Caption = top.card.SetName,
                        ImageUrl = ImageRel(top.card)
                    }
                ]
            };
        }
    }
}
