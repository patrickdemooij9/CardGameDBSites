using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class CardListService
    {
        private const string DefaultListName = "Trade";

        private readonly CardListRepository _cardListRepository;
        private readonly MemberInfoService _memberInfoService;

        public CardListService(CardListRepository cardListRepository, MemberInfoService memberInfoService)
        {
            _cardListRepository = cardListRepository;
            _memberInfoService = memberInfoService;
        }

        public IEnumerable<CardList> GetByUser(int userId)
        {
            return _cardListRepository.GetByUser(userId);
        }

        public CardList? Get(int id)
        {
            return _cardListRepository.Get(id);
        }

        public CardList? GetPublic(int id)
        {
            var list = _cardListRepository.Get(id);
            if (list is null || !list.IsPublic) return null;
            return list;
        }

        public int Create(string name, int userId, string? description = null, bool isPublic = false)
        {
            var list = new CardList(name, userId)
            {
                Description = description,
                IsPublic = isPublic
            };
            return _cardListRepository.Add(list);
        }

        public bool Update(int listId, int userId, string name, string? description, bool isPublic)
        {
            var list = _cardListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId) return false;

            list.Name = name;
            list.Description = description;
            list.IsPublic = isPublic;
            _cardListRepository.Update(list);
            return true;
        }

        public bool Delete(int listId, int userId)
        {
            var list = _cardListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId) return false;

            _cardListRepository.Delete(listId);
            return true;
        }

        public CardListItem? AddItem(int listId, int userId, int cardId, int? variantId, int amount)
        {
            var list = _cardListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId) return null;

            var existingItem = list.Items.FirstOrDefault(it => it.CardId == cardId && it.VariantId == variantId);
            if (existingItem != null)
            {
                existingItem.Amount = amount;
                _cardListRepository.UpdateItem(existingItem);
                return existingItem;
            }

            var item = new CardListItem
            {
                ListId = listId,
                CardId = cardId,
                VariantId = variantId,
                Amount = amount,
                AddedDate = DateTime.UtcNow
            };
            _cardListRepository.AddItem(item);
            return item;
        }

        public bool RemoveItem(int listId, int userId, int itemId)
        {
            var list = _cardListRepository.Get(listId);
            if (list is null || list.CreatedBy != userId) return false;

            var item = list.Items.FirstOrDefault(it => it.Id == itemId);
            if (item is null) return false;

            _cardListRepository.RemoveItem(itemId);
            return true;
        }

        public IEnumerable<CardListItem> GetItemsByCard(int cardId, int userId)
        {
            return _cardListRepository.GetItemsByCard(cardId, userId);
        }

        public void EnsureDefaultListExists(int userId)
        {
            var lists = _cardListRepository.GetByUser(userId);
            if (!lists.Any())
            {
                Create(DefaultListName, userId);
            }
        }
    }
}
