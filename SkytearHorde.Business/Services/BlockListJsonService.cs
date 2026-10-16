using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Services;

namespace SkytearHorde.Business.Services
{
    public class BlockListJsonService
    {
        public const string KeyAlias = "key";
        public const string ContentTypeKeyAlias = "contentTypeKey";

        private readonly IContentTypeService _contentTypeService;

        public BlockListJsonService(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public string Serialize(IEnumerable<IReadOnlyDictionary<string, string>> items, Guid defaultContentTypeKey)
        {
            var layout = new JsonArray();
            var contentData = new JsonArray();
            var expose = new JsonArray();

            foreach (var item in items)
            {
                var contentTypeKey = item.TryGetValue(ContentTypeKeyAlias, out var typeKey) ? Guid.Parse(typeKey) : defaultContentTypeKey;
                var key = (item.TryGetValue(KeyAlias, out var existingKey) ? Guid.Parse(existingKey) : Guid.NewGuid()).ToString();
                var contentType = _contentTypeService.Get(contentTypeKey)
                    ?? throw new InvalidOperationException($"Element type {contentTypeKey} does not exist");

                var values = new JsonArray();
                foreach (var (alias, value) in item)
                {
                    if (alias is KeyAlias or ContentTypeKeyAlias) continue;

                    var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(it => it.Alias == alias)
                        ?? throw new InvalidOperationException($"Element type {contentType.Alias} has no property {alias}");
                    values.Add(new JsonObject
                    {
                        ["editorAlias"] = propertyType.PropertyEditorAlias,
                        ["culture"] = null,
                        ["segment"] = null,
                        ["alias"] = alias,
                        ["value"] = value
                    });
                }

                contentData.Add(new JsonObject { ["contentTypeKey"] = contentTypeKey.ToString(), ["key"] = key, ["values"] = values });
                layout.Add(new JsonObject { ["contentKey"] = key, ["settingsKey"] = null });
                expose.Add(new JsonObject { ["contentKey"] = key, ["culture"] = null, ["segment"] = null });
            }

            return new JsonObject
            {
                ["contentData"] = contentData,
                ["settingsData"] = new JsonArray(),
                ["expose"] = expose,
                ["layout"] = new JsonObject { ["Umbraco.BlockList"] = layout }
            }.ToJsonString();
        }

        public List<Dictionary<string, string>> Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];

            var root = JsonNode.Parse(json);
            var blocksByKey = root?["contentData"]?.AsArray()
                .OfType<JsonObject>()
                .ToDictionary(it => it["key"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase) ?? [];
            var layoutKeys = root?["layout"]?["Umbraco.BlockList"]?.AsArray()
                .OfType<JsonObject>()
                .Select(it => it["contentKey"]!.GetValue<string>()) ?? [];

            return layoutKeys
                .Where(blocksByKey.ContainsKey)
                .Select(key => ToItem(blocksByKey[key]))
                .ToList();
        }

        private static Dictionary<string, string> ToItem(JsonObject block)
        {
            var item = new Dictionary<string, string>
            {
                [KeyAlias] = block["key"]!.GetValue<string>(),
                [ContentTypeKeyAlias] = block["contentTypeKey"]!.GetValue<string>()
            };

            foreach (var property in block["values"]?.AsArray().OfType<JsonObject>() ?? [])
            {
                var value = property["value"];
                if (value is null) continue;

                item[property["alias"]!.GetValue<string>()] = value.GetValueKind() == JsonValueKind.String
                    ? value.GetValue<string>()
                    : value.ToJsonString();
            }
            return item;
        }
    }
}
