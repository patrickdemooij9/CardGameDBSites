using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Api.Delivery.Json;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace SkytearHorde.DeliveryApi
{
    public class SeoToolkitJsonTypeResolver : DeliveryApiJsonTypeResolver
    {
        public override Type[] GetDerivedTypes(JsonTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo.Type == typeof(IApiContentResponse))
            {
                return [typeof(ApiContentResponse), typeof(SeoToolkitContentResponse)];
            }

            return base.GetDerivedTypes(jsonTypeInfo);
        }
    }
}
