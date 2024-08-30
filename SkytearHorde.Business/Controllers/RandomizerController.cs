using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SkytearHorde.Business.Controllers
{
    public class RandomizerController : SurfaceController
    {
        private readonly RandomizeService _randomizeService;

        public RandomizerController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, RandomizeService randomizeService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _randomizeService = randomizeService;
        }

        public IActionResult RandomizeScenario(RandomizerPostModel postModel)
        {
            if (UmbracoContext.Content!.GetById(postModel.PageId) is not ContentPage page) return NotFound();

            var randomizerComponent = page.Grid?.SelectMany(it => it.Areas.SelectMany(a => a)).Select(it => it.Content).OfType<Randomizer>().FirstOrDefault();
            if (randomizerComponent is null) return NotFound();

            var setIds = postModel.Sets.Where(it => it.Value).Select(it => it.Key).ToArray();
            var result = new Dictionary<string, string>();
            foreach (var resultType in randomizerComponent.Results.ToItems<RandomizerResult>())
            {
                var randomizeResult = _randomizeService.RandomizeScenario(new RandomizeRequest
                {
                    SetIds = setIds,
                    Requirements = resultType.Requirements.ToItems<ISquadRequirementConfig>().Select(it => it.GetRequirement()).ToArray(),
                    ReturnDistinctAttribute = resultType.Attribute?.Name
                });
                result.Add(resultType.Title!, randomizeResult);
            }

            return View("~/Views/Partials/components/randomizerResult.cshtml", new RandomizerResultViewModel
            {
                Result = result
            });
        }
    }
}
