namespace SkytearHorde.Business.Facts.Generators
{
    public class NewestTraitsGenerator : FactGeneratorBase
    {
        public override string Key => "newest-traits";

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            if (context.ReleasedSetsOldToNew.Count == 0) return null;
            var newest = context.ReleasedSetsOldToNew[^1];

            var cardsBySet = context.Cards
                .GroupBy(c => c.SetId)
                .ToDictionary(g => g.Key, g => g.ToArray());

            var earlierTraits = context.ReleasedSetsOldToNew
                .Take(context.ReleasedSetsOldToNew.Count - 1)
                .SelectMany(set => cardsBySet.GetValueOrDefault(set.Id, []))
                .SelectMany(GetTraits)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newTraits = cardsBySet.GetValueOrDefault(newest.Id, [])
                .SelectMany(GetTraits)
                .Where(t => !earlierTraits.Contains(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToArray();

            if (newTraits.Length == 0) return null;

            var setLabel = string.IsNullOrWhiteSpace(newest.SetCode) ? newest.DisplayName : newest.SetCode.ToUpperInvariant();

            return new GameFact
            {
                Key = Key,
                Hook = $"Do you know how many new traits the {setLabel} set introduced?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.List,
                        Heading = $"NEW IN {setLabel}",
                        Title = "New traits",
                        BigValue = newTraits.Length.ToString(),
                        BigLabel = "NEW TRAITS",
                        Items = newTraits
                    }
                ]
            };
        }
    }
}
