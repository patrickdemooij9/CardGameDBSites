using CardGameDBSites.API.Models.Community;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/community")]
    [ApiExplorerSettings(GroupName = "Community")]
    public class CommunityApiController : Controller
    {
        private readonly ContentCreatorService _contentCreatorService;
        private readonly ISiteService _siteService;

        public CommunityApiController(ContentCreatorService contentCreatorService, ISiteService siteService)
        {
            _contentCreatorService = contentCreatorService;
            _siteService = siteService;
        }

        [HttpGet("posts")]
        [ProducesResponseType(typeof(PagedCommunityBlogPostsApiModel), 200)]
        public IActionResult GetPosts(int page = 1, int pageSize = 30)
        {
            var allBlogPosts = _contentCreatorService.GetAllBlogPosts()
                .OrderByDescending(it => it.PublishedDate)
                .ToArray();
            var totalItems = allBlogPosts.Length;
            var skip = (page - 1) * pageSize;
            var blogPosts = allBlogPosts.Skip(skip).Take(pageSize).ToArray();

            var creators = _contentCreatorService.GetAll()?.ToDictionary(it => it.Id, it => it)
                           ?? new Dictionary<int, ContentCreatorItem>();

            var siteSettings = _siteService.GetSettings().FirstChild<SiteSettings>();
            var defaultAuthor = siteSettings?.DefaultCreatorName ?? "Author";
            var defaultImageUrl = siteSettings?.DefaultCreatorImage?.Url(mode: UrlMode.Absolute);

            var items = blogPosts.Select(post =>
            {
                creators.TryGetValue(post.CreatorId, out var creator);
                return new CommunityBlogPostApiModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    ImageUrl = post.ImageUrl ?? defaultImageUrl,
                    Url = post.Url,
                    PublishedDate = post.PublishedDate,
                    Author = creator?.DisplayName ?? defaultAuthor,
                    TagType = creator?.TagType ?? "Blog post"
                };
            }).ToArray();

            return Ok(new PagedCommunityBlogPostsApiModel
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items
            });
        }
    }
}
