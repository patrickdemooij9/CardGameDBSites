using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;

namespace SkytearHorde.Business.ContentFinders
{
    public class ListContentFinder : IContentFinder
    {
        private readonly DeckListService _deckListService;
        private readonly IFileService _fileService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;

        public ListContentFinder(DeckListService deckListService, IFileService fileService, IUmbracoContextFactory umbracoContextFactory, ISiteService siteService)
        {
            _deckListService = deckListService;
            _fileService = fileService;
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
        }

        public Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {
            if (request.Uri.Segments.Length != 3)
                return Task.FromResult(false);

            if (request.Uri.Segments[1] != "list/")
                return Task.FromResult(false);

            if (!int.TryParse(request.Uri.Segments[2], out var listId))
                return Task.FromResult(false);

            var list = _deckListService.Get(listId);
            if (list is null)
                return Task.FromResult(false);

            var template = _fileService.GetTemplate("listDetail");
            if (template is null)
                return Task.FromResult(false);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var listDetailPage = _siteService.GetRoot().FirstChild<ListDetail>();

            request.SetPublishedContent(listDetailPage);
            request.SetTemplate(template);
            return Task.FromResult(true);
        }
    }
}
