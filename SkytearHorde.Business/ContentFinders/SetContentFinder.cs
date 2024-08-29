using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SkytearHorde.Business.ContentFinders
{
    public class SetContentFinder : IContentFinder
    {
        private readonly ISiteService _siteService;
        private readonly CardService _cardService;
        private readonly IFileService _fileService;

        public SetContentFinder(ISiteService siteService, CardService cardService, IFileService fileService)
        {
            _siteService = siteService;
            _cardService = cardService;
            _fileService = fileService;
        }

        public Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {
            var setOverview = _siteService.GetSetOverview();
            if (setOverview is null) 
                return Task.FromResult(false);

            if (!request.Uri.AbsolutePath.StartsWith(setOverview.Url()))
                return Task.FromResult(false);

            var requestedSet = request.Uri.Segments[^1];
            var set = _cardService.GetAllSets().FirstOrDefault(it => requestedSet.Equals(it.UrlSegment));
            if (set is null)
                return Task.FromResult(false);

            var template = _fileService.GetTemplate("setDetail");
            if (template is null)
                return Task.FromResult(false);

            request.SetPublishedContent(set);
            request.SetTemplate(template);

            return Task.FromResult(true);
        }
    }
}
