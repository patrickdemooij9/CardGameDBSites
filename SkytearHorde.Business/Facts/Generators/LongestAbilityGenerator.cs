using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Business.Facts.Generators
{
    public class LongestAbilityGenerator : FactGeneratorBase
    {
        public override string Key => "longest-ability";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            var top3 = context.Cards
                .OrderByDescending(x => x.GetMultipleCardAttributeValue("Abilities")?.FirstOrDefault()?.Length ?? 0)
                .Take(3)
                .ToArray();

            if (top3.Length == 0) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Do you know which card has the longest ability text in {ScopeLabel(context)}?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "LONGEST ABILITY",
                        Title = top3[0].DisplayName,
                        BigValue = top3[0].GetMultipleCardAttributeValue("Abilities")!.First().Length.ToString(),
                        BigLabel = "CHARACTERS",
                        ImageUrl = ImageRel(top3[0]),
                    },
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "SECOND LONGEST ABILITY",
                        Title = top3[1].DisplayName,
                        BigValue = top3[1].GetMultipleCardAttributeValue("Abilities")!.First().Length.ToString(),
                        BigLabel = "CHARACTERS",
                        ImageUrl = ImageRel(top3[1]),
                    },
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = "THIRD LONGEST ABILITY",
                        Title = top3[2].DisplayName,
                        BigValue = top3[2].GetMultipleCardAttributeValue("Abilities")!.First().Length.ToString(),
                        BigLabel = "CHARACTERS",
                        ImageUrl = ImageRel(top3[2]),
                    }
                ]
            };
        }
    }
}
