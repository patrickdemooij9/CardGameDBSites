using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class DeckFolderService
    {
        private readonly DeckFolderRepository _deckFolderRepository;
        private readonly DeckRepository _deckRepository;

        public DeckFolderService(DeckFolderRepository deckFolderRepository, DeckRepository deckRepository)
        {
            _deckFolderRepository = deckFolderRepository;
            _deckRepository = deckRepository;
        }

        public DeckFolder? Get(int id)
        {
            return _deckFolderRepository.Get(id);
        }

        public IEnumerable<DeckFolder> GetByUser(int userId, int siteId)
        {
            return _deckFolderRepository.GetByUser(userId, siteId);
        }

        public Dictionary<int, int> GetDeckCounts(int userId, int siteId)
        {
            return _deckRepository.GetFolderDeckCounts(userId, siteId);
        }

        public int Create(string name, string? description, int userId, int siteId)
        {
            return _deckFolderRepository.Add(new DeckFolder(name, userId, siteId)
            {
                Description = description
            });
        }

        public bool Update(int id, int userId, string name, string? description)
        {
            var folder = _deckFolderRepository.Get(id);
            if (folder is null || folder.CreatedBy != userId) return false;

            folder.Name = name;
            folder.Description = description;
            _deckFolderRepository.Update(folder);
            return true;
        }

        public bool Delete(int id, int userId, int siteId)
        {
            var folder = _deckFolderRepository.Get(id);
            if (folder is null || folder.CreatedBy != userId) return false;

            // Unfile the decks first (keeps the decks, just clears their folder) so no deck points at a missing folder.
            var deckIds = _deckRepository.GetDeckIdsByFolder(id, userId, siteId).ToArray();
            if (deckIds.Length > 0)
            {
                _deckRepository.SetFolder(deckIds, null, userId, siteId);
            }

            _deckFolderRepository.Delete(id);
            return true;
        }

        /// <summary>
        /// Moves the given decks into a folder (or unfiles them when <paramref name="folderId"/> is null).
        /// A null/invalid target folder that is not owned by the user is rejected. Ownership of the decks
        /// themselves is enforced in the update, so only the caller's own decks are affected.
        /// </summary>
        public bool MoveDecks(int? folderId, int userId, int siteId, IEnumerable<int> deckIds)
        {
            if (folderId.HasValue)
            {
                var folder = _deckFolderRepository.Get(folderId.Value);
                if (folder is null || folder.CreatedBy != userId || folder.SiteId != siteId) return false;
            }

            _deckRepository.SetFolder(deckIds, folderId, userId, siteId);
            return true;
        }
    }
}
