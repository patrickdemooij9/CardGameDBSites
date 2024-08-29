using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;

namespace SkytearHorde.Business.Processors
{
    public class DeckTrackingProcessor
    {
        private readonly DeckService _deckService;
        private readonly DeckViewRepository _deckViewRepository;

        private readonly Dictionary<int, int> _views;

        public DeckTrackingProcessor(DeckService deckService, DeckViewRepository deckViewRepository)
        {
            _views = new Dictionary<int, int>();

            _deckService = deckService;
            _deckViewRepository = deckViewRepository;
        }

        public void Process()
        {
            if (_views.Count == 0) return;

            var copiedViews = new Dictionary<int, int>(_views);
            _views.Clear();

            var existingIds = _deckService.Exists(copiedViews.Keys);
            foreach (var deck in copiedViews)
            {
                // Invalid deck id
                if (!existingIds.ContainsKey(deck.Key) || !existingIds[deck.Key]) continue;

                _deckViewRepository.AddViews(deck.Key, deck.Value);
            }
        }

        public void AddDeckView(int deckId)
        {
            if (!_views.ContainsKey(deckId))
            {
                _views.Add(deckId, 1);
                return;
            }
            _views[deckId]++;
        }
    }
}
