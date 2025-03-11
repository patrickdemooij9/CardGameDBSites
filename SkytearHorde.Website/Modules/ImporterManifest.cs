using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;

namespace SkytearHorde.Modules
{
    internal class ManifestLoader : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<ManifestFilter>();
        }
    }

    internal class ManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "Importer",
                Version = "1.0.0",
                Scripts = new[]
                {
                    "/App_Plugins/Importer/importer.controller.js"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
