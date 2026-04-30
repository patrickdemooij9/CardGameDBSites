using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Interfaces;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Business.Services
{
    public class ArchetypeService
    {
        private readonly ArchetypeRepository _archetypeRepository;
        private readonly DeckRepository _deckRepository;
        private readonly IEnumerable<IGameAdapter> _gameAdapters;

        public ArchetypeService(
            ArchetypeRepository archetypeRepository,
            DeckRepository deckRepository,
            IEnumerable<IGameAdapter> gameAdapters)
        {
            _archetypeRepository = archetypeRepository;
            _deckRepository = deckRepository;
            _gameAdapters = gameAdapters;
        }

        public List<Archetype> GetArchetypesForSite(int siteId, int? formatId = null)
        {
            return _archetypeRepository.GetBySite(siteId, formatId);
        }

        public void AssignArchetypeToDeck(int deckId, Guid archetypeId)
        {
            var archetype = _archetypeRepository.Get(archetypeId);
            if (archetype is null)
                throw new ArgumentException($"Archetype {archetypeId} not found.");

            _archetypeRepository.AssignToDeck(deckId, archetypeId);
        }

        public string? SuggestArchetype(int deckId)
        {
            var deck = _deckRepository.Get(DeckStatus.None, deckId).FirstOrDefault();
            if (deck is null) return null;

            var adapter = _gameAdapters.FirstOrDefault(a =>
                a.SiteId == deck.SiteId &&
                (a.FormatId is null || a.FormatId == deck.TypeId));

            return adapter?.SuggestArchetype(deck);
        }
    }
}
