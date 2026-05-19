using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.DeliveryApi
{
    public class SeoToolkitResponseBuilder : ApiContentResponseBuilder
    {
        private readonly IMetaFieldsService _metaFieldsService;

        public SeoToolkitResponseBuilder(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor, IMetaFieldsService metaFieldsService) : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor)
        {
            _metaFieldsService = metaFieldsService;
        }

        protected override IApiContentResponse Create(
        IPublishedContent content,
        string name,
        IApiContentRoute route,
        IDictionary<string, object?> properties)
        {
            var cultures = GetCultures(content);
            var metaFields = _metaFieldsService.Get(content, true).Fields.ToDictionary(it => it.Key.Alias, it => it.Value);

            return new SeoToolkitContentResponse(
                content.Key,
                name,
                content.ContentType.Alias,
                content.CreateDate,
                content.UpdateDate,
                route,
                properties,
                cultures,
                metaFields);
        }
    }
}
