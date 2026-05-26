using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace SkytearHorde.DeliveryApi
{
    public static class CustomSwaggerRouteUmbracoBuilderExtensions
    {
        public static IUmbracoBuilder ConfigureMySwaggerRoute(this IUmbracoBuilder builder)
        {
            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                // include this line if you do NOT want the Swagger docs at /umbraco/swagger
                options.PipelineFilters.RemoveAll(filter => filter is SwaggerRouteTemplatePipelineFilter);

                // setup your own Swagger routes
                options.AddFilter(new CustomSwaggerRouteTemplatePipelineFilter("MyApi"));
            });
            return builder;
        }
    }
}
