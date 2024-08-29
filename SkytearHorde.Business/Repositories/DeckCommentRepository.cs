using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using System.Linq;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckCommentRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public DeckCommentRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public DeckComment? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            var entity = scope.Database.FirstOrDefault<DeckCommentDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckCommentDBModel>()
                .Where<DeckCommentDBModel>(it => it.Id == id && it.SiteId == siteId));
            return entity != null ? Map(entity) : null;
        }

        public DeckComment[] GetByDeck(int deckId)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.Fetch<DeckCommentDBModel>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<DeckCommentDBModel>()
                    .Where<DeckCommentDBModel>(it => it.DeckId == deckId && it.SiteId == siteId))
                .Select(Map)
                .ToArray();
        }

        public DeckComment[] GetLatest(int amount)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.SkipTake<DeckCommentDBModel>(0, amount, scope.SqlContext.Sql()
                    .SelectAll()
                    .From<DeckCommentDBModel>()
                    .LeftJoin<DeckDBModel>().On<DeckDBModel, DeckCommentDBModel>((left, right) => left.Id == right.DeckId)
                    .Where<DeckDBModel>(it => !it.IsDeleted && it.SiteId == siteId)
                    .OrderByDescending<DeckCommentDBModel>(it => it.CreatedAt))
                .Select(Map)
                .ToArray();
        }

        public int Add(DeckComment comment)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = Map(comment);
            scope.Database.Insert(entity);
            scope.Complete();

            return entity.Id;
        }

        public void Delete(DeckComment comment)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete(Map(comment));
            scope.Complete();
        }

        private DeckCommentDBModel Map(DeckComment model)
        {
            return new DeckCommentDBModel
            {
                Id = model.Id,
                DeckId = model.DeckId,
                ParentId = model.ParentId,
                SiteId = model.SiteId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Comment = model.Comment,
            };
        }

        private DeckComment Map(DeckCommentDBModel model)
        {
            return new DeckComment
            {
                Id = model.Id,
                DeckId = model.DeckId,
                ParentId = model.ParentId,
                SiteId = model.SiteId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Comment = model.Comment,
            };
        }
    }
}
