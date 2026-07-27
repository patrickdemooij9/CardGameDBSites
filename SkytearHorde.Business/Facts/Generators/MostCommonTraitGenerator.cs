using Humanizer;

namespace SkytearHorde.Business.Facts.Generators
{
    public class MostCommonTraitGenerator : FactGeneratorBase
    {
        public override string Key => "most-common-traits";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            var top3 = context.Cards
                .SelectMany(GetTraits)
                .GroupBy(it => it)
                .OrderByDescending(it => it.Count())
                .Take(3)
                .ToArray();

            return new GameFact
            {
                Key = Key,
                Hook = "Do you know which trait is the most common in the game?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "MOST COMMON TRAIT",
                        Title = top3[0].Key,
                        BigValue = top3[0].Count().ToString(),
                        BigLabel = "TIMES IN THE GAME",
                        ImageUrl = ImageRel(context.Cards.First(it => GetTraits(it).Contains(top3[0].Key)))
                    },
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "SECOND MOST COMMON",
                        Title = top3[1].Key,
                        BigValue = top3[1].Count().ToString(),
                        BigLabel = "TIMES IN THE GAME",
                        ImageUrl = ImageRel(context.Cards.First(it => GetTraits(it).Contains(top3[1].Key)))
                    },
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "THIRD MOST COMMON TRAIT",
                        Title = top3[2].Key,
                        BigValue = top3[2].Count().ToString(),
                        BigLabel = "TIMES IN THE GAME",
                        ImageUrl = ImageRel(context.Cards.First(it => GetTraits(it).Contains(top3[2].Key)))
                    },
                ]
            };
        }
    }
}
