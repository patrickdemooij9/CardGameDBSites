using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class DeckListService
    {
        private readonly DeckService _deckService;
        private readonly DeckListRepository _deckListRepository;

        public DeckListService(DeckService deckService, DeckListRepository deckListRepository)
        {
            _deckService = deckService;
            _deckListRepository = deckListRepository;
        }

        public void CreateNewDeckList(string name, int userId, IEnumerable<int> decks)
        {
            _deckListRepository.Add(new DeckList(name, userId)
            {
                DeckIds = decks.ToList()
            });
        }

        public void Update(DeckList list)
        {
            _deckListRepository.Update(list);
        }

        public void AddToList(int listId, int userId, int deckId)
        {
            if (!_deckService.Exists(new int[] { deckId }).First().Value) return;

            var list = _deckListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId || list.DeckIds.Contains(deckId)) return;

            list.DeckIds.Add(deckId);
            _deckListRepository.Update(list);
        }

        public void RemoveFromList(int listId, int userId, int deckId)
        {
            if (!_deckService.Exists(new int[] { deckId }).First().Value) return;

            var list = _deckListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId || !list.DeckIds.Contains(deckId)) return;

            list.DeckIds.Remove(deckId);
            _deckListRepository.Update(list);
        }

        public DeckList? Get(int id)
        {
            return _deckListRepository.Get(id);
        }

        public IEnumerable<DeckList> GetByUser(int userId)
        {
            return _deckListRepository.GetByUser(userId);
        }
    }
}
