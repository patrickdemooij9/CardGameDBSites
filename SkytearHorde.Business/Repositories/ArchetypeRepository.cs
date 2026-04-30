using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class ArchetypeRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public ArchetypeRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public Guid Create(Archetype archetype)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = new ArchetypeDBModel
            {
                Id = archetype.Id == Guid.Empty ? Guid.NewGuid() : archetype.Id,
                SiteId = archetype.SiteId,
                FormatId = archetype.FormatId,
                Name = archetype.Name,
                Description = archetype.Description
            };

            scope.Database.Insert(model);
            scope.Complete();

            return model.Id;
        }

        public Archetype? Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();

            var model = scope.Database.FirstOrDefault<ArchetypeDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<ArchetypeDBModel>()
                    .Where<ArchetypeDBModel>(a => a.Id == id));

            return model is null ? null : Map(model);
        }

        public List<Archetype> GetBySite(int siteId, int? formatId = null)
        {
            using var scope = _scopeProvider.CreateScope();

            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<ArchetypeDBModel>()
                .Where<ArchetypeDBModel>(a => a.SiteId == siteId);

            if (formatId.HasValue)
            {
                var fid = formatId.Value;
                sql = sql.Where<ArchetypeDBModel>(a => a.FormatId == fid || a.FormatId == null);
            }

            return scope.Database.Fetch<ArchetypeDBModel>(sql).Select(Map).ToList();
        }

        public List<Archetype> GetForDeck(int deckId)
        {
            using var scope = _scopeProvider.CreateScope();

            return scope.Database.Fetch<ArchetypeDBModel>(
                "SELECT a.* FROM Archetype a INNER JOIN DeckArchetype da ON a.Id = da.ArchetypeId WHERE da.DeckId = @0",
                deckId)
                .Select(Map)
                .ToList();
        }

        public void AssignToDeck(int deckId, Guid archetypeId)
        {
            using var scope = _scopeProvider.CreateScope();

            var existing = scope.Database.FirstOrDefault<DeckArchetypeDBModel>(
                scope.SqlContext.Sql()
                    .SelectAll()
                    .From<DeckArchetypeDBModel>()
                    .Where<DeckArchetypeDBModel>(da => da.DeckId == deckId && da.ArchetypeId == archetypeId));

            if (existing is null)
            {
                scope.Database.Insert(new DeckArchetypeDBModel { DeckId = deckId, ArchetypeId = archetypeId });
            }

            scope.Complete();
        }

        private static Archetype Map(ArchetypeDBModel model) => new Archetype
        {
            Id = model.Id,
            SiteId = model.SiteId,
            FormatId = model.FormatId,
            Name = model.Name,
            Description = model.Description
        };
    }
}
