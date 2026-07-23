using SkytearHorde.Business.Facts;

namespace SkytearHorde.Business.Services
{
    public class FactService
    {
        private readonly IEnumerable<IFactGenerator> _generators;
        private readonly CardService _cardService;

        public FactService(IEnumerable<IFactGenerator> generators, CardService cardService)
        {
            _generators = generators;
            _cardService = cardService;
        }

        private FactContext BuildContext(string? setCode)
        {
            var allSets = _cardService.GetAllSets().ToArray();
            var released = allSets
                .Where(s => s.HasBeenReleased)
                .OrderBy(s => s.SortOrder)
                .ToArray();

            IEnumerable<SkytearHorde.Entities.Models.Business.Card> cards = _cardService.GetAll();
            if (!string.IsNullOrWhiteSpace(setCode))
            {
                // Filter to the requested set (no match => empty, so generators return nothing).
                var set = allSets.FirstOrDefault(s => string.Equals(s.SetCode, setCode, StringComparison.OrdinalIgnoreCase));
                var setId = set?.Id ?? -1;
                cards = cards.Where(c => c.SetId == setId);
            }

            return new FactContext { Cards = cards.ToArray(), ReleasedSetsOldToNew = released };
        }

        /// <summary>Runs every automatic generator over a single shared context, dropping any that can't produce a fact.</summary>
        /// <param name="setCode">Optional set code (e.g. "jtl") to scope the cards to; null uses all cards.</param>
        public IReadOnlyList<GameFact> GetFacts(string? setCode = null)
        {
            var context = BuildContext(setCode);
            return _generators
                .Where(g => g.IsAutomatic)
                .Select(g => TryGenerate(g, context, null))
                .OfType<GameFact>()
                .ToArray();
        }

        /// <summary>Runs a single named generator (used for on-demand / parameterized facts). Null if unknown or not produced.</summary>
        /// <param name="setCode">Optional set code (e.g. "jtl") to scope the cards to; null uses all cards.</param>
        public GameFact? GetFact(string key, IReadOnlyDictionary<string, string>? parameters, string? setCode = null)
        {
            var generator = _generators.FirstOrDefault(g => g.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return generator is null ? null : TryGenerate(generator, BuildContext(setCode), parameters);
        }

        private static GameFact? TryGenerate(IFactGenerator generator, FactContext context, IReadOnlyDictionary<string, string>? parameters)
        {
            try
            {
                return generator.Generate(context, parameters);
            }
            catch
            {
                return null;
            }
        }
    }
}
