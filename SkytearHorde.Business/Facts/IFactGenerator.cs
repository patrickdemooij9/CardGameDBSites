namespace SkytearHorde.Business.Facts
{
    /// <summary>
    /// A pluggable "Did you know?" fact generator. Add a new one by implementing this
    /// interface and registering it in <c>MainComposer</c>.
    /// </summary>
    public interface IFactGenerator
    {
        /// <summary>Stable url-friendly key, e.g. "highest-health-unit".</summary>
        string Key { get; }

        /// <summary>Whether this fact appears in the parameterless facts listing.</summary>
        bool IsAutomatic { get; }

        /// <summary>Returns the fact, or null when it cannot be produced for the current site/data/parameters.</summary>
        GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters);
    }
}
