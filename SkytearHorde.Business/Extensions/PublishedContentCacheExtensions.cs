using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace SkytearHorde.Business.Extensions
{
    public static class PublishedContentCacheExtensions
    {
        public static IEnumerable<IPublishedContent> GetAtRoot(this IPublishedContentCache cache)
        {
            var navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
            if (!navigationQueryService.TryGetRootKeys(out var rootKeys)) return [];

            return rootKeys.Select(key => cache.GetById(key)).OfType<IPublishedContent>();
        }
    }
}
