using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class ContentCreatorBlogPostRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public ContentCreatorBlogPostRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public IEnumerable<ContentCreatorBlogPost> GetAll(int siteId)
        {
            using var ctx = _scopeProvider.CreateScope();
            return ctx.Database.Fetch<ContentCreatorBlogPostDBModel>(ctx.SqlContext.Sql()
                .SelectAll()
                .From<ContentCreatorBlogPostDBModel>()
                .Where<ContentCreatorBlogPostDBModel>(it => it.SiteId == siteId)).Select(it => new ContentCreatorBlogPost
            {
                Id = it.BlogId,
                Title = it.Title,
                ImageUrl = it.ImageUrl,
                Url = it.Url,
                CreatorId = it.CreatorId,
                PublishedDate = it.PublishedDate
            }).ToArray();
        }

        public void Insert(ContentCreatorBlogPost contentCreatorBlogPost, int siteId)
        {
            using var ctx = _scopeProvider.CreateScope(autoComplete: true);
            ctx.Database.Insert(new ContentCreatorBlogPostDBModel
            {
                BlogId = contentCreatorBlogPost.Id,
                Title = contentCreatorBlogPost.Title,
                Url = contentCreatorBlogPost.Url,
                ImageUrl = contentCreatorBlogPost.ImageUrl,
                CreatorId = contentCreatorBlogPost.CreatorId,
                PublishedDate = contentCreatorBlogPost.PublishedDate,
                SiteId = siteId
            });
        }

        public void Remove(string id)
        {
            using var ctx = _scopeProvider.CreateScope(autoComplete: true);
            ctx.Database.Delete<ContentCreatorBlogPostDBModel>(ctx.Database
                .First<ContentCreatorBlogPostDBModel>(ctx.SqlContext.Sql()
                    .SelectAll()
                    .From<ContentCreatorBlogPostDBModel>()
                    .Where<ContentCreatorBlogPostDBModel>(it => it.BlogId == id)));
        }
    }
}
