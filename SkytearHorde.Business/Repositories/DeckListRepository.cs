using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckListRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DeckListRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public DeckList Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            var list = scope.Database.FirstOrDefault<DeckListDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckListDBModel>()
                .Where<DeckListDBModel>(it => it.Id == id));

            var items = scope.Database.Fetch<DeckListItemDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckListItemDBModel>()
                .Where<DeckListItemDBModel>(it => it.ListId == id));

            return Map(list, items);
        }

        public IEnumerable<DeckList> GetByUser(int userId)
        {
            using var scope = _scopeProvider.CreateScope();

            var lists = scope.Database.Fetch<DeckListDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckListDBModel>()
                .Where<DeckListDBModel>(it => it.CreatedBy == userId));

            var items = scope.Database.Fetch<DeckListItemDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckListItemDBModel>()
                .WhereIn<DeckListItemDBModel>(it => it.ListId, lists.Select(it => it.Id)))
            .GroupBy(it => it.ListId);

            foreach(var list in lists)
            {
                var listItems = items.FirstOrDefault(it => it.Key.Equals(list.Id));
                yield return Map(list, listItems ?? Enumerable.Empty<DeckListItemDBModel>());
            }
        }

        public void Add(DeckList list)
        {
            using var scope = _scopeProvider.CreateScope();

            var listModel = new DeckListDBModel
            {
                Name = list.Name,
                Description = list.Description,
                CreatedBy = list.CreatedBy,
                CreatedDate = list.CreatedDate
            };
            scope.Database.Insert(listModel);

            foreach (var item in list.DeckIds)
            {
                scope.Database.Insert(new DeckListItemDBModel
                {
                    ListId = listModel.Id,
                    DeckId = item
                });
            }
            scope.Complete();
        }

        public void Update(DeckList list)
        {
            var currentList = Get(list.Id);

            using var scope = _scopeProvider.CreateScope();

            var listModel = new DeckListDBModel
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                CreatedBy = list.CreatedBy,
                CreatedDate = list.CreatedDate
            };
            scope.Database.Update(listModel);

            foreach (var newDeckId in list.DeckIds.Where(it => !currentList.DeckIds.Contains(it)))
            {
                scope.Database.Insert(new DeckListItemDBModel { ListId = listModel.Id, DeckId = newDeckId });
            }

            foreach (var deletedDeckId in currentList.DeckIds.Where(it => !list.DeckIds.Contains(it)))
            {
                scope.Database.Delete<DeckListItemDBModel>(scope.SqlContext.Sql()
                    .From<DeckListItemDBModel>()
                    .Where<DeckListItemDBModel>(it => it.ListId == listModel.Id && it.DeckId == deletedDeckId));
            }
            scope.Complete();
        }

        private DeckList Map(DeckListDBModel list, IEnumerable<DeckListItemDBModel> items)
        {
            return new DeckList(list.Name, list.CreatedBy)
            {
                Id = list.Id,
                Description = list.Description,
                DeckIds = items.Select(it => it.DeckId).ToList(),
                CreatedDate = list.CreatedDate
            };
        }
    }
}
