using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckViewIndexAndTotalViewsMigration : MigrationBase
    {
        public DeckViewIndexAndTotalViewsMigration(IMigrationContext context) : base(context) { }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "TotalViews"))
            {
                Create.Column("TotalViews").OnTable("Deck").AsInt32().WithDefaultValue(0).Do();
            }

            if (!IndexExists("IX_DeckView_DeckId_Date"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_DeckView_DeckId_Date ON DeckView (DeckId, Date)");
            }

            // Aggregate historical views (older than 30 days) into TotalViews before deleting
            Database.Execute(@"UPDATE d SET d.TotalViews = ISNULL(d.TotalViews, 0) + agg.ViewCount
FROM Deck d
INNER JOIN (
    SELECT DeckId, SUM(Views) AS ViewCount
    FROM DeckView
    WHERE Date < DATEADD(day, -30, GETUTCDATE())
    GROUP BY DeckId
) agg ON d.Id = agg.DeckId");

            // Delete historical data older than 30 days
            Database.Execute("DELETE FROM DeckView WHERE Date < DATEADD(day, -30, GETUTCDATE())");
        }
    }
}
