using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckFolderRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public DeckFolderRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public DeckFolder? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            var folder = scope.Database.FirstOrDefault<DeckFolderDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckFolderDBModel>()
                .Where<DeckFolderDBModel>(it => it.Id == id));

            return folder is null ? null : Map(folder);
        }

        public IEnumerable<DeckFolder> GetByUser(int userId, int siteId)
        {
            using var scope = _scopeProvider.CreateScope();

            var folders = scope.Database.Fetch<DeckFolderDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckFolderDBModel>()
                .Where<DeckFolderDBModel>(it => it.CreatedBy == userId && it.SiteId == siteId));

            return folders.Select(Map).ToArray();
        }

        public int Add(DeckFolder folder)
        {
            using var scope = _scopeProvider.CreateScope();

            var folderModel = new DeckFolderDBModel
            {
                Name = folder.Name,
                Description = folder.Description,
                CreatedBy = folder.CreatedBy,
                CreatedDate = folder.CreatedDate,
                SiteId = folder.SiteId
            };
            scope.Database.Insert(folderModel);
            scope.Complete();

            folder.Id = folderModel.Id;
            return folderModel.Id;
        }

        public void Update(DeckFolder folder)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Update(new DeckFolderDBModel
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                CreatedBy = folder.CreatedBy,
                CreatedDate = folder.CreatedDate,
                SiteId = folder.SiteId
            });
            scope.Complete();
        }

        public void Delete(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete<DeckFolderDBModel>(scope.SqlContext.Sql()
                .From<DeckFolderDBModel>()
                .Where<DeckFolderDBModel>(it => it.Id == id));
            scope.Complete();
        }

        private DeckFolder Map(DeckFolderDBModel folder)
        {
            return new DeckFolder(folder.Name, folder.CreatedBy, folder.SiteId)
            {
                Id = folder.Id,
                Description = folder.Description,
                CreatedDate = folder.CreatedDate
            };
        }
    }
}
