using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DeliveryApi;

namespace SkytearHorde.DeliveryApi
{
    public class DeliveryApiComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IApiContentResponseBuilder, SeoToolkitResponseBuilder>();
        }
    }
}
