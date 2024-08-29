using Examine;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup
{
    public class ExamineSiteIdComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ExamineSiteIdComponent(IExamineManager examineManager, IUmbracoContextFactory umbracoContextFactory)
        {
            _examineManager = examineManager;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public void Initialize()
        {
            if (!_examineManager.TryGetIndex("ExternalIndex", out var index))
            {
                return;
            }

            index.TransformingIndexValues += Index_TransformingIndexValues;
        }

        private void Index_TransformingIndexValues(object? sender, IndexingItemEventArgs e)
        {
            if (!int.TryParse(e.ValueSet.Id, out var contentId)) return;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var content = ctx.UmbracoContext.Content!.GetById(contentId);
            if (content is null) return;

            var siteId = content.Root().FirstChild<Settings>()?.FirstChild<SiteSettings>()?.SiteId;
            if (siteId is null) return;

            var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());
            updatedValues["siteId"] = new List<object>() { siteId };
            e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
        }

        public void Terminate()
        {
        }
    }
}
