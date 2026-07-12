namespace SkytearHorde.Business.Facts.Generators
{
    public class HighestHealthUnitGenerator : FactGeneratorBase
    {
        public override string Key => "highest-health-unit";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            var top = context.Cards
                .Where(IsUnit)
                .Select(card => (card, health: GetInt(card, "Health")))
                .Where(x => x.health > 0)
                .OrderByDescending(x => x.health)
                .ThenBy(x => x.card.DisplayName)
                .FirstOrDefault();

            if (top.card is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Did you know {top.card.DisplayName} is the highest-health unit in the game, with {top.health} health?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "HIGHEST HEALTH",
                        Title = top.card.DisplayName,
                        BigValue = top.health.ToString(),
                        BigLabel = "HEALTH",
                        ImageUrl = ImageRel(top.card)
                    }
                ]
            };
        }
    }
}
