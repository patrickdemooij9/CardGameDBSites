using Umbraco.Cms.Api.Common.OpenApi;

namespace SkytearHorde.DeliveryApi
{
    public class CustomSwaggerRouteTemplatePipelineFilter : SwaggerRouteTemplatePipelineFilter
    {
        public CustomSwaggerRouteTemplatePipelineFilter(string name) : base(name)
        {
        }

        /// <summary>
        /// This is how you change the route template for the Swagger docs.
        /// </summary>
        protected override string SwaggerRouteTemplate(IApplicationBuilder applicationBuilder) => "swagger/{documentName}/swagger.json";

        /// <summary>
        /// This is how you change the route for the Swagger UI.
        /// </summary>
        protected override string SwaggerUiRoutePrefix(IApplicationBuilder applicationBuilder) => "swagger";

        /// <summary>
        /// This is how you configure Swagger to be available always.
        /// Please note that this is NOT recommended.
        /// </summary>
        protected override bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder) => true;
    }
}
