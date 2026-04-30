using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class MetaService
    {
        private readonly TournamentRepository _tournamentRepository;
        private readonly TournamentEntrantRepository _entrantRepository;
        private readonly ArchetypeRepository _archetypeRepository;

        public MetaService(
            TournamentRepository tournamentRepository,
            TournamentEntrantRepository entrantRepository,
            ArchetypeRepository archetypeRepository)
        {
            _tournamentRepository = tournamentRepository;
            _entrantRepository = entrantRepository;
            _archetypeRepository = archetypeRepository;
        }

        public List<ArchetypeMetaResult> GetTopDecks(int siteId, int? formatId = null)
        {
            var tournaments = _tournamentRepository.GetBySite(siteId, formatId);
            var archetypes = _archetypeRepository.GetBySite(siteId, formatId)
                .ToDictionary(a => a.Id);

            var entrantsByArchetype = new Dictionary<Guid, int>();

            foreach (var tournament in tournaments)
            {
                var entrants = _entrantRepository.GetByTournament(tournament.Id);
                foreach (var entrant in entrants.Where(e => e.DeckId.HasValue))
                {
                    var deckArchetypes = _archetypeRepository.GetForDeck(entrant.DeckId!.Value);
                    foreach (var archetype in deckArchetypes)
                    {
                        entrantsByArchetype.TryGetValue(archetype.Id, out var count);
                        entrantsByArchetype[archetype.Id] = count + 1;
                    }
                }
            }

            return entrantsByArchetype
                .OrderByDescending(kv => kv.Value)
                .Where(kv => archetypes.ContainsKey(kv.Key))
                .Select(kv => new ArchetypeMetaResult
                {
                    Archetype = archetypes[kv.Key],
                    DeckCount = kv.Value
                })
                .ToList();
        }

        public List<ArchetypeMetaResult> GetTrendingDecks(int siteId, int? formatId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd)
        {
            var allTournaments = _tournamentRepository.GetBySite(siteId, formatId);
            var archetypes = _archetypeRepository.GetBySite(siteId, formatId)
                .ToDictionary(a => a.Id);

            var currentTournaments = allTournaments.Where(t => t.Date >= currentStart && t.Date <= currentEnd).ToList();
            var previousTournaments = allTournaments.Where(t => t.Date >= previousStart && t.Date <= previousEnd).ToList();

            var currentCounts = CountArchetypeEntrants(currentTournaments);
            var previousCounts = CountArchetypeEntrants(previousTournaments);

            var allArchetypeIds = currentCounts.Keys.Union(previousCounts.Keys).Distinct();

            return allArchetypeIds
                .Where(id => archetypes.ContainsKey(id))
                .Select(id =>
                {
                    currentCounts.TryGetValue(id, out var curr);
                    previousCounts.TryGetValue(id, out var prev);
                    return new ArchetypeMetaResult
                    {
                        Archetype = archetypes[id],
                        DeckCount = curr,
                        PreviousDeckCount = prev,
                        Trend = curr - prev
                    };
                })
                .OrderByDescending(r => r.Trend)
                .ToList();
        }

        private Dictionary<Guid, int> CountArchetypeEntrants(List<TournamentEvent> tournaments)
        {
            var counts = new Dictionary<Guid, int>();
            foreach (var tournament in tournaments)
            {
                var entrants = _entrantRepository.GetByTournament(tournament.Id);
                foreach (var entrant in entrants.Where(e => e.DeckId.HasValue))
                {
                    var deckArchetypes = _archetypeRepository.GetForDeck(entrant.DeckId!.Value);
                    foreach (var archetype in deckArchetypes)
                    {
                        counts.TryGetValue(archetype.Id, out var count);
                        counts[archetype.Id] = count + 1;
                    }
                }
            }
            return counts;
        }
    }

    public class ArchetypeMetaResult
    {
        public required Archetype Archetype { get; set; }
        public int DeckCount { get; set; }
        public int PreviousDeckCount { get; set; }
        public int Trend { get; set; }
    }
}
