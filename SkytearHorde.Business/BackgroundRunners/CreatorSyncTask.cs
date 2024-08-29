using Microsoft.Extensions.Logging;
using SkytearHorde.Business.CreatorSyncs;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class CreatorSyncTask : RecurringHostedServiceBase
    {
        private readonly ContentCreatorService _contentCreatorService;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILogger _logger;

        public CreatorSyncTask(ILogger<CreatorSyncTask> logger, ContentCreatorService contentCreatorService, ISiteService siteService, ISiteAccessor siteAccessor, IUmbracoContextFactory umbracoContextFactory) : base(logger, TimeSpan.FromHours(4), TimeSpan.FromMinutes(1))
        {
            _contentCreatorService = contentCreatorService;
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _umbracoContextFactory = umbracoContextFactory;
            _logger = logger;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach(var siteId in _siteService.GetAllSites())
            {
                _siteAccessor.SetSiteId(siteId);

                var contentCreators = _contentCreatorService.GetAll();
                if (contentCreators is null) continue;

                var currentPosts = _contentCreatorService.GetAllBlogPosts().GroupBy(it => it.CreatorId).ToDictionary(it => it.Key, it => it.ToArray());

                foreach (var contentCreator in contentCreators) 
                {
                    var currentPostsOfCreator = currentPosts.ContainsKey(contentCreator.Id) ? currentPosts[contentCreator.Id] : Array.Empty<ContentCreatorBlogPost>();
                    var posts = GetItems(contentCreator).ToArray();

                    foreach(var newPost in posts.Where(it => !currentPostsOfCreator.Any(c => c.Id == it.Id)))
                    {
                        _contentCreatorService.AddBlogPost(new ContentCreatorBlogPost
                        {
                            Id = newPost.Id,
                            Title = newPost.Title,
                            Url = newPost.Url,
                            ImageUrl = newPost.ImageUrl,
                            CreatorId = contentCreator.Id,
                            PublishedDate = newPost.PublishedDate
                        });
                    }
                    
                    /*foreach(var removedPost in currentPostsOfCreator.Where(it => !posts.Any(p => p.Id == it.Id)))
                    {
                        _contentCreatorService.RemoveBlogPost(removedPost.Id);
                    }*/
                }
            }
            return Task.CompletedTask;
        }

        private IEnumerable<CreatorSyncItem> GetItems(ContentCreatorItem contentCreatorItem)
        {
            var syncMethod = contentCreatorItem.SyncMethod.ToItems<IContentCreatorSyncConfig>().FirstOrDefault();
            if (syncMethod is null) return Enumerable.Empty<CreatorSyncItem>();
            if (syncMethod is ContentCreatorRssSyncConfig rssConfig)
            {
                return new CreatorRssSync(rssConfig, _logger).GetAll();
            }
            return Enumerable.Empty<CreatorSyncItem>();
        }
    }
}
