using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using System.Linq;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class CardCommentRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteAccessor _siteAccessor;

        public CardCommentRepository(IScopeProvider scopeProvider, ISiteAccessor siteAccessor)
        {
            _scopeProvider = scopeProvider;
            _siteAccessor = siteAccessor;
        }

        public CardComment? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            var entity = scope.Database.FirstOrDefault<CardCommentDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<CardCommentDBModel>()
                .Where<CardCommentDBModel>(it => it.Id == id && it.SiteId == siteId));
            return entity != null ? Map(entity) : null;
        }

        public CardComment[] GetByCard(int cardId)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.Fetch<CardCommentDBModel>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<CardCommentDBModel>()
                    .Where<CardCommentDBModel>(it => it.CardId == cardId && it.SiteId == siteId))
                .Select(Map)
                .ToArray();
        }

        public CardComment[] GetLatest(int amount)
        {
            using var scope = _scopeProvider.CreateScope();

            var siteId = _siteAccessor.GetSiteId();
            return scope.Database.SkipTake<CardCommentDBModel>(0, amount, scope.SqlContext.Sql()
                    .SelectAll()
                    .From<CardCommentDBModel>()
                    .Where<CardCommentDBModel>(it => it.SiteId == siteId)
                    .OrderByDescending<CardCommentDBModel>(it => it.CreatedAt))
                .Select(Map)
                .ToArray();
        }

        public int Add(CardComment comment)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = Map(comment);
            scope.Database.Insert(entity);
            scope.Complete();

            return entity.Id;
        }

        public void Delete(CardComment comment)
        {
            using var scope = _scopeProvider.CreateScope();

            scope.Database.Delete(Map(comment));
            scope.Complete();
        }

        private CardCommentDBModel Map(CardComment model)
        {
            return new CardCommentDBModel
            {
                Id = model.Id,
                CardId = model.CardId,
                ParentId = model.ParentId,
                SiteId = model.SiteId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Comment = model.Comment,
            };
        }

        private CardComment Map(CardCommentDBModel model)
        {
            return new CardComment
            {
                Id = model.Id,
                CardId = model.CardId,
                ParentId = model.ParentId,
                SiteId = model.SiteId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Comment = model.Comment,
            };
        }
    }
}
