using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CardListRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public CardListRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public CardList? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            var list = scope.Database.FirstOrDefault<CardListDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardListDBModel>()
                .Where<CardListDBModel>(it => it.Id == id));

            if (list is null) return null;

            var items = scope.Database.Fetch<CardListItemDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardListItemDBModel>()
                .Where<CardListItemDBModel>(it => it.ListId == id));

            return Map(list, items);
        }

        public IEnumerable<CardList> GetByUser(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var lists = scope.Database.Fetch<CardListDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardListDBModel>()
                .Where<CardListDBModel>(it => it.CreatedBy == userId));

            var listIds = lists.Select(it => it.Id).ToArray();
            var items = listIds.Length > 0
                ? scope.Database.Fetch<CardListItemDBModel>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<CardListItemDBModel>()
                    .WhereIn<CardListItemDBModel>(it => it.ListId, listIds))
                    .GroupBy(it => it.ListId)
                : Enumerable.Empty<IGrouping<int, CardListItemDBModel>>();

            foreach (var list in lists)
            {
                var listItems = items.FirstOrDefault(it => it.Key == list.Id);
                yield return Map(list, listItems ?? Enumerable.Empty<CardListItemDBModel>());
            }
        }

        public int Add(CardList list)
        {
            using var scope = _scopeProvider.CreateScope();

            var listModel = new CardListDBModel
            {
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedBy = list.CreatedBy,
                CreatedDate = list.CreatedDate
            };
            scope.Database.Insert(listModel);
            scope.Complete();

            return listModel.Id;
        }

        public void Update(CardList list)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Update(new CardListDBModel
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedBy = list.CreatedBy,
                CreatedDate = list.CreatedDate
            });
            scope.Complete();
        }

        public void Delete(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete<CardListItemDBModel>(scope.SqlContext.Sql()
                .From<CardListItemDBModel>()
                .Where<CardListItemDBModel>(it => it.ListId == id));

            scope.Database.Delete<CardListDBModel>(id);
            scope.Complete();
        }

        public void AddItem(CardListItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Insert(new CardListItemDBModel
            {
                ListId = item.ListId,
                CardId = item.CardId,
                VariantId = item.VariantId,
                Amount = item.Amount,
                AddedDate = item.AddedDate
            });
            scope.Complete();
        }

        public void UpdateItem(CardListItem item)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Update(new CardListItemDBModel
            {
                Id = item.Id,
                ListId = item.ListId,
                CardId = item.CardId,
                VariantId = item.VariantId,
                Amount = item.Amount,
                AddedDate = item.AddedDate
            });
            scope.Complete();
        }

        public void RemoveItem(int itemId)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Delete<CardListItemDBModel>(itemId);
            scope.Complete();
        }

        public IEnumerable<CardListItem> GetItemsByCard(int cardId, int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var userLists = scope.Database.Fetch<CardListDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardListDBModel>()
                .Where<CardListDBModel>(it => it.CreatedBy == userId));

            var listIds = userLists.Select(it => it.Id).ToArray();
            if (listIds.Length == 0) return Enumerable.Empty<CardListItem>();

            var items = scope.Database.Fetch<CardListItemDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardListItemDBModel>()
                .Where<CardListItemDBModel>(it => it.CardId == cardId)
                .WhereIn<CardListItemDBModel>(it => it.ListId, listIds));

            return items.Select(MapItem);
        }

        private CardList Map(CardListDBModel list, IEnumerable<CardListItemDBModel> items)
        {
            return new CardList(list.Name, list.CreatedBy)
            {
                Id = list.Id,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedDate = list.CreatedDate,
                Items = items.Select(MapItem).ToList()
            };
        }

        private CardListItem MapItem(CardListItemDBModel item)
        {
            return new CardListItem
            {
                Id = item.Id,
                ListId = item.ListId,
                CardId = item.CardId,
                VariantId = item.VariantId,
                Amount = item.Amount,
                AddedDate = item.AddedDate
            };
        }
    }
}
