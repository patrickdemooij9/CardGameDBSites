using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Constants = Umbraco.Cms.Core.Constants;

namespace SkytearHorde.Business.Startup
{
    public class ConfigureExternalIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        public void Configure(string? name, LuceneDirectoryIndexOptions options)
        {
            if (name?.Equals(Constants.UmbracoIndexes.ExternalIndexName) is true)
            {
                options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET);
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
        {

        }
    }
}
