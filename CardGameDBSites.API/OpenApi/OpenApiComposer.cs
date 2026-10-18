using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace CardGameDBSites.API.OpenApi
{
    public class OpenApiComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.Configure<SwaggerGenOptions>(options => options.DocumentFilter<RequestModelRequiredDocumentFilter>());
        }
    }
}
