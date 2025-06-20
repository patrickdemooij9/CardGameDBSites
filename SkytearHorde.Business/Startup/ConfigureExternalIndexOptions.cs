using Examine;
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

                //TODO: Make this generic somehow
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.OldAidalon.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Omniworks.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Collective.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Remnants.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.OldAidalon.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Omniworks.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Collective.Amount", FieldDefinitionTypes.Integer));
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Remnants.Amount", FieldDefinitionTypes.Integer));
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
        {

        }
    }
}
