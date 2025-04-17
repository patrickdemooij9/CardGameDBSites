using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Core.Composing;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class MigrationComponent : IComponent
    {
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public MigrationComponent(
            ICoreScopeProvider coreScopeProvider,
            IMigrationPlanExecutor migrationPlanExecutor,
            IKeyValueService keyValueService,
            IRuntimeState runtimeState)
        {
            _coreScopeProvider = coreScopeProvider;
            _migrationPlanExecutor = migrationPlanExecutor;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            var migrationPlan = new MigrationPlan("Decks");

            migrationPlan.From(string.Empty)
                .To<InitialDeckMigration>("v1")
                .To<DeckDescriptionMigration>("v2")
                .To<DeckViewMigration>("v3")
                .To<DeckVersionMigration>("v4")
                .To<DeckListMigration>("v5")
                .To<PageViewTrackingMigration>("v6")
                .To<MemberSiteIdMigration>("v7")
                .To<DeckCardGroupAndSlotMigration>("v8")
                .To<DeckSiteIdMigration>("v9")
                .To<ContentCreatorBlogPostMigration>("v10")
                .To<AnonymousDeckMigration>("v11")
                .To<AdServerMigration>("v12")
                .To<DeckDeleteMigration>("v13")
                .To<DeckLikeMigration>("v14")
                .To<DeckCalculateMigration>("v15")
                .To<DeckCommentMigration>("v16")
                .To<CommentSiteIdMigration>("v17")
                .To<DeckTypeMigration>("v18")
                .To<CollectionItemMigration>("v19")
                .To<CardPriceMigration>("v20")
                .To<CollectionPackMigration>("v21")
                .To<RedditDailyCardMigration>("v22")
                .To<CardBaseVariantMigration>("v23")
                .To<VariantReferenceMigration>("v24")
                .To<CardSetReferenceMigration>("v25")
                .To<DeckCardChildMigration>("v26")
                .To<DuplicateCollectionCardMigration>("v27");

            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);
        }

        public void Terminate()
        {
        }
    }
}
