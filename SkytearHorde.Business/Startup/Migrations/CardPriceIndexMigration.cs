using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class CardPriceIndexMigration : MigrationBase
    {
        public CardPriceIndexMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!IndexExists("IX_CardPriceRecord_CardIdIsLatest"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_CardPriceRecord_CardIdIsLatest ON CardPriceRecord (CardId,IsLatest)");
            }
        }
    }
}
