using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DeliveryApi;

namespace SkytearHorde.DeliveryApi
{
    public class DeliveryApiComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IApiContentResponseBuilder, SeoToolkitResponseBuilder>();
            builder.Services.PostConfigure<JsonOptions>(Constants.JsonOptionsNames.DeliveryApi,
                options => options.JsonSerializerOptions.TypeInfoResolver = new SeoToolkitJsonTypeResolver());
        }
    }
}
