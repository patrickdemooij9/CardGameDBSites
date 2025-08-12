using Examine;
using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.Startup.Indexes
{
    internal class ConfigureCardIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IOptions<IndexCreatorSettings> _settings;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ConfigureCardIndexOptions(IOptions<IndexCreatorSettings> settings, IUmbracoContextFactory umbracoContextFactory)
        {
            _settings = settings;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public void Configure(string? name, LuceneDirectoryIndexOptions options)
        {
            if (name?.Equals("CardIndex") is false)
            {
                return;
            }

            var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET);

            options.FieldDefinitions = new(
                new("id", FieldDefinitionTypes.Integer),
                new("name", FieldDefinitionTypes.FullText)
            );

            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.OldAidalon.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Omniworks.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Collective.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Requirement.Remnants.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.OldAidalon.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Omniworks.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Collective.Amount", FieldDefinitionTypes.Integer));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition("CustomField.Pip Source.Remnants.Amount", FieldDefinitionTypes.Integer));

            options.UnlockIndex = true;

            if (_settings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
            {
                // if this directory factory is enabled then a snapshot deletion policy is required
                options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options) => throw new NotImplementedException();
    }
}
