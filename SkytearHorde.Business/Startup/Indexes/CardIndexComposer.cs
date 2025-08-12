using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;

namespace SkytearHorde.Business.Startup.Indexes
{
    internal class CardIndexComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddExamineLuceneIndex<ExamineCardIndex, ConfigurationEnabledDirectoryFactory>("CardIndex");

            builder.Services.ConfigureOptions<ConfigureCardIndexOptions>();

            builder.Services.AddSingleton<CardIndexValueSetBuilder>();

            builder.Services.AddSingleton<IIndexPopulator, CardIndexPopulator>();

            builder.AddNotificationHandler<ContentCacheRefresherNotification, CardIndexNotificationHandler>();
        }
    }
}
