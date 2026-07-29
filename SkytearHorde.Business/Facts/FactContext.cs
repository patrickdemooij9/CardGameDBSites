using Card = SkytearHorde.Entities.Models.Business.Card;
using Set = SkytearHorde.Entities.Generated.Set;

namespace SkytearHorde.Business.Facts
{
    /// <summary>
    /// Pre-loaded, site-scoped card/set data shared across all generators for one request
    /// (so the heavy <c>CardService.GetAll()</c> runs once).
    /// </summary>
    public class FactContext
    {
        public required IReadOnlyList<Card> Cards { get; init; }
        public required IReadOnlyList<Set> ReleasedSetsOldToNew { get; init; }

        /// <summary>The set the facts are scoped to, or null when they cover the whole game.</summary>
        public Set? Set { get; init; }
    }
}
