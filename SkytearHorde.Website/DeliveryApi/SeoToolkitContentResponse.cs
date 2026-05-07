using Umbraco.Cms.Core.Models.DeliveryApi;

namespace SkytearHorde.DeliveryApi
{
    public class SeoToolkitContentResponse : ApiContentResponse
    {
        public SeoToolkitContentResponse(Guid id, string name, string contentType, DateTime createDate, DateTime updateDate, IApiContentRoute route, IDictionary<string, object?> properties, IDictionary<string, IApiContentRoute> cultures, Dictionary<string, object> seoToolkit) : base(id, name, contentType, createDate, updateDate, route, properties, cultures)
        {
            SeoToolkit = seoToolkit;
        }

        public Dictionary<string, object> SeoToolkit { get; set; } = new();
    }
}
