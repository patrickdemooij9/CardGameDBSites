using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CollectionCardIndexesMigration : MigrationBase
    {
        public CollectionCardIndexesMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!IndexExists("IX_Deck_UserId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_Deck_UserId ON CollectionCard (UserId)");
            }

            if (!IndexExists("IX_Deck_VariantId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_Deck_VariantId ON CollectionCard (VariantId)");
            }
        }
    }
}
