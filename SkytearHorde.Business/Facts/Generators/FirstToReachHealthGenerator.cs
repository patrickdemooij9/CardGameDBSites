namespace SkytearHorde.Business.Facts.Generators
{
    /// <summary>
    /// Requires an explicit <c>value</c> parameter (the health threshold X); not auto-listed.
    /// </summary>
    public class FirstToReachHealthGenerator : FactGeneratorBase
    {
        public override string Key => "first-to-reach-health";
        public override bool IsAutomatic => false;

        public override GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            if (parameters is null
                || !parameters.TryGetValue("value", out var raw)
                || !int.TryParse(raw, out var threshold)
                || threshold <= 0)
            {
                return null;
            }

            // Release order is approximated by set sort order, with the card's CMS create date as tiebreak.
            var setOrder = context.ReleasedSetsOldToNew
                .Select((set, index) => (set.Id, index))
                .ToDictionary(x => x.Id, x => x.index);

            var first = context.Cards
                .Where(IsUnit)
                .Where(c => GetInt(c, "Health") >= threshold)
                .OrderBy(c => setOrder.TryGetValue(c.SetId, out var order) ? order : int.MaxValue)
                .ThenBy(c => c.CreatedDate)
                .FirstOrDefault();

            if (first is null) return null;

            return new GameFact
            {
                Key = Key,
                Hook = $"Do you know which card was the first to reach {threshold} health?",
                Slides =
                [
                    new FactSlide
                    {
                        Kind = FactSlideKind.HeroCard,
                        Heading = $"FIRST TO {threshold} HEALTH",
                        Title = first.DisplayName,
                        BigValue = GetInt(first, "Health").ToString(),
                        BigLabel = "HEALTH",
                        Caption = first.SetName,
                        ImageUrl = ImageRel(first)
                    }
                ]
            };
        }
    }
}
