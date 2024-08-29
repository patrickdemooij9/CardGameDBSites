using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.CustomCardMaker;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class CardMakerController : SurfaceController
    {
        private readonly CardMaker _cardMaker;

        public CardMakerController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, CardMaker cardMaker) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _cardMaker = cardMaker;
        }

        public async Task<IActionResult> RenderCard(RenderCardPostModel model)
        {
            var bytes = await _cardMaker.Generate(model);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Content($"<img class='mt-16 h-96 rounded-md max-w-full' src='data:image/png;base64,{Convert.ToBase64String(bytes)}'/>");
            }
            else
            {
                return File(bytes, "image/png", "YourCard.png");
            }
        }

        /*public async Task<IActionResult> RenderCard()
        {
            return File(await _cardMaker.Generate(), "image/png");
        }*/
    }
}
