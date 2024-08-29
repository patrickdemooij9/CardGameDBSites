using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class ContentCreatorService
    {
        private readonly ContentCreatorBlogPostRepository _contentCreatorBlogPostRepository;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;

        public ContentCreatorService(ContentCreatorBlogPostRepository contentCreatorBlogPostRepository,
            IUmbracoContextFactory umbracoContextFactory, ISiteService siteService, ISiteAccessor siteAccessor)
        {
            _contentCreatorBlogPostRepository = contentCreatorBlogPostRepository;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
        }

        public ContentCreatorItem[]? GetAll()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            return _siteService.GetRoot().FirstChild<Data>()?.FirstChild<ContentCreatorContainer>()?.Children<ContentCreatorItem>()?.ToArray();
        }

        public IEnumerable<ContentCreatorBlogPost> GetBlogPosts(int amount, int skip)
        {
            return GetAllBlogPosts().OrderByDescending(it => it.PublishedDate).Skip(skip).Take(amount);
        }

        public IEnumerable<ContentCreatorBlogPost> GetAllBlogPosts()
        {
            var communityBlogPosts = _contentCreatorBlogPostRepository.GetAll(_siteAccessor.GetSiteId());
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            communityBlogPosts = communityBlogPosts.Concat(_siteService.GetRoot().FirstChild<BlogOverview>()?.Children<BlogDetail>()?.Select(it => new ContentCreatorBlogPost
            {
                Id = it.Id.ToString(),
                Url = it.Url(),
                Title = it.Title,
                CreatorId = -1,
                PublishedDate = it.PublishDate,
                ImageUrl = it.Image?.Url()
            }) ?? Enumerable.Empty<ContentCreatorBlogPost>());
            return communityBlogPosts;
        }

        public void AddBlogPost(ContentCreatorBlogPost blogPost)
        {
            _contentCreatorBlogPostRepository.Insert(blogPost, _siteAccessor.GetSiteId());
        }

        public void RemoveBlogPost(string blogPostId)
        {
            _contentCreatorBlogPostRepository.Remove(blogPostId);
        }
    }
}
