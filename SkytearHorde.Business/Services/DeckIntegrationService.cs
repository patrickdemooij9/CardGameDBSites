using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Business.Services
{
    public class DeckIntegrationService
    {
        private readonly DeckRepository _deckRepository;

        public DeckIntegrationService(DeckRepository deckRepository)
        {
            _deckRepository = deckRepository;
        }

        public bool ValidateDeckForGame(int deckId, int siteId, int formatId)
        {
            var deck = _deckRepository.Get(DeckStatus.None, deckId).FirstOrDefault();
            if (deck is null) return false;

            return deck.SiteId == siteId && deck.TypeId == formatId;
        }
    }
}
