using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class DeckTotalLikesMigration : MigrationBase
    {
        public DeckTotalLikesMigration(IMigrationContext context) : base(context) { }

        protected override void Migrate()
        {
            if (!ColumnExists("Deck", "TotalLikes"))
            {
                Create.Column("TotalLikes").OnTable("Deck").AsInt32().WithDefaultValue(0).Do();
            }

            // Populate TotalLikes from existing DeckLike data
            Database.Execute(@"UPDATE d SET d.TotalLikes = ISNULL(agg.LikeCount, 0)
FROM Deck d
LEFT JOIN (
    SELECT DeckId, COUNT(*) AS LikeCount
    FROM DeckLike
    GROUP BY DeckId
) agg ON d.Id = agg.DeckId");
        }
    }
}
