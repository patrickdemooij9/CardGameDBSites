using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace SkytearHorde.Business.Extensions
{
    public static class BlockListExtensions
    {
        public static IEnumerable<T> ToItems<T>(this BlockListModel? model) where T : IPublishedElement
        {
            if (typeof(T) == typeof(PublishedElement))
            {
                return model?.OfType<BlockListItem<T>>().Select(it => it.Content) ?? Enumerable.Empty<T>();
            }
            return model?.OfType<BlockListItem>().Select(it => (T)it.Content) ?? Enumerable.Empty<T>();
        }
    }
}
