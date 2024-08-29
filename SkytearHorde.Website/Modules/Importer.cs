using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace SkytearHorde.Modules
{
    public class Importer : IContentAppFactory
    {
        public ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is IContent content && content.ContentType.Alias == Data.ModelTypeAlias)
            {
                return new ContentApp
                {
                    Alias = "importer",
                    View = "/App_Plugins/Importer/importer.html",
                    Name = "Importer",
                    Icon = "icon-document"
                };
            }
            return null;
        }
    }

    public class ImporterStartup : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ContentApps().Append<Importer>();
        }
    }
}
