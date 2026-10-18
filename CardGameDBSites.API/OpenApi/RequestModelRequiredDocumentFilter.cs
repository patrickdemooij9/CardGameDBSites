using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CardGameDBSites.API.OpenApi
{
    // Undoes Umbraco's RequireNonNullablePropertiesSchemaFilter for request models, whose unset fields get server defaults.
    public class RequestModelRequiredDocumentFilter : IDocumentFilter
    {
        private const string DocumentName = "default";

        public void Apply(OpenApiDocument document, DocumentFilterContext context)
        {
            if (context.DocumentName != DocumentName || document.Components?.Schemas is null) return;

            var requestTypes = CollectTypes(context.ApiDescriptions
                .SelectMany(it => it.ParameterDescriptions)
                .Where(it => it.Source == BindingSource.Body)
                .Select(it => it.Type));
            var responseTypes = CollectTypes(context.ApiDescriptions
                .SelectMany(it => it.SupportedResponseTypes)
                .Select(it => it.Type));

            foreach (var type in requestTypes.Except(responseTypes))
            {
                if (!context.SchemaRepository.TryLookupByType(type, out var reference)) continue;
                if (!document.Components.Schemas.TryGetValue(reference.Reference.Id!, out var found)) continue;
                if (found is not OpenApiSchema { Required: not null } schema) continue;

                var explicitlyRequired = type.GetProperties()
                    .Where(IsExplicitlyRequired)
                    .Select(GetJsonName)
                    .ToHashSet();
                schema.Required.IntersectWith(explicitlyRequired);
            }
        }

        private static HashSet<Type> CollectTypes(IEnumerable<Type?> roots)
        {
            var found = new HashSet<Type>();
            var pending = new Stack<Type>(roots.OfType<Type>());
            while (pending.TryPop(out var type))
            {
                if (type.IsArray)
                {
                    pending.Push(type.GetElementType()!);
                    continue;
                }
                if (type.IsGenericType)
                {
                    foreach (var argument in type.GetGenericArguments()) pending.Push(argument);
                    continue;
                }
                if (!type.IsClass || type.Namespace?.StartsWith("System") is not false || !found.Add(type)) continue;

                foreach (var property in type.GetProperties()) pending.Push(property.PropertyType);
            }
            return found;
        }

        private static bool IsExplicitlyRequired(PropertyInfo property)
            => property.IsDefined(typeof(RequiredMemberAttribute)) || property.IsDefined(typeof(RequiredAttribute));

        private static string GetJsonName(PropertyInfo property)
            => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? property.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>()?.PropertyName
                ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
    }
}
