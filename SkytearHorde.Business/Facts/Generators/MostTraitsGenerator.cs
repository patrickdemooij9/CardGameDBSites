namespace SkytearHorde.Business.Facts.Generators
{
    public class MostTraitsGenerator : FactGeneratorBase
    {
        public override string Key => "most-traits";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            var top = context.Cards
                .Select(card => (card, traits: GetTraits(card)))
                .Where(x => x.traits.Length > 0)
                .OrderByDescending(x => x.traits.Length)
                .ThenBy(x => x.card.DisplayName)
                .FirstOrDefault();

            if (top.card is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Did you know {top.card.DisplayName} has more traits than any other card, with {top.traits.Length}?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "MOST TRAITS",
                        Title = top.card.DisplayName,
                        BigValue = top.traits.Length.ToString(),
                        BigLabel = "TRAITS",
                        Caption = string.Join("  •  ", top.traits),
                        ImageUrl = ImageRel(top.card),
                        Items = top.traits
                    }
                ]
            };
        }
    }
}
