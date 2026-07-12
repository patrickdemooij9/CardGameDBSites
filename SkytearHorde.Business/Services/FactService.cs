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

        private FactContext BuildContext()
        {
            var cards = _cardService.GetAll().ToArray();
            var sets = _cardService.GetAllSets()
                .Where(s => s.HasBeenReleased)
                .OrderBy(s => s.SortOrder)
                .ToArray();

            return new FactContext { Cards = cards, ReleasedSetsOldToNew = sets };
        }

        /// <summary>Runs every automatic generator over a single shared context, dropping any that can't produce a fact.</summary>
        public IReadOnlyList<GameFact> GetFacts()
        {
            var context = BuildContext();
            return _generators
                .Where(g => g.IsAutomatic)
                .Select(g => TryGenerate(g, context, null))
                .OfType<GameFact>()
                .ToArray();
        }

        /// <summary>Runs a single named generator (used for on-demand / parameterized facts). Null if unknown or not produced.</summary>
        public GameFact? GetFact(string key, IReadOnlyDictionary<string, string>? parameters)
        {
            var generator = _generators.FirstOrDefault(g => g.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return generator is null ? null : TryGenerate(generator, BuildContext(), parameters);
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
