namespace SkytearHorde.Business.Facts.Generators
{
    public class HighestPowerUnitGenerator : FactGeneratorBase
    {
        public override string Key => "highest-power-unit";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            var top = context.Cards
                .Where(IsUnit)
                .Select(card => (card, power: GetInt(card, "Power")))
                .Where(x => x.power > 0)
                .OrderByDescending(x => x.power)
                .ThenBy(x => x.card.DisplayName)
                .FirstOrDefault();

            if (top.card is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = "Do you know which unit has the most power in the game?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "HIGHEST POWER",
                        Title = top.card.DisplayName,
                        BigValue = top.power.ToString(),
                        BigLabel = "POWER",
                        ImageUrl = ImageRel(top.card)
                    }
                ]
            };
        }
    }
}
