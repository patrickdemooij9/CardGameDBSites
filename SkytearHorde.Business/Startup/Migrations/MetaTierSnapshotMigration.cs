using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    /// <summary>
    /// Turns MetaSnapshots into a weekly time series. Historical weeks are filled in afterwards by
    /// MetaSnapshotService.BackfillForPeriod, not here, because that needs to be re-runnable.
    /// </summary>
    public class MetaTierSnapshotMigration : MigrationBase
    {
        public MetaTierSnapshotMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("MetaSnapshots", "SnapshotDateUtc"))
            {
                Create.Column("SnapshotDateUtc").OnTable("MetaSnapshots").AsDateTime().Nullable().Do();

                // DATEDIFF(week) counts Sunday boundaries, so floor from a known Monday instead.
                Database.Execute(
                    "UPDATE MetaSnapshots " +
                    "SET SnapshotDateUtc = DATEADD(day, (DATEDIFF(day, '19000101', LastUpdatedUtc) / 7) * 7, '19000101') " +
                    "WHERE SnapshotDateUtc IS NULL");

                Alter.Table("MetaSnapshots").AlterColumn("SnapshotDateUtc").AsDateTime().NotNullable().Do();
            }

            if (!ColumnExists("MetaSnapshots", "IsLatest"))
            {
                Create.Column("IsLatest").OnTable("MetaSnapshots").AsBoolean().Nullable().Do();
                // Pre-migration there was exactly one row per period, so all of them are the latest.
                Database.Execute("UPDATE MetaSnapshots SET IsLatest = 1 WHERE IsLatest IS NULL");
                Alter.Table("MetaSnapshots").AlterColumn("IsLatest").AsBoolean().NotNullable().Do();
            }

            // Backfilled as 0; BackfillForPeriod populates them.
            foreach (var column in new[] { "Draws", "EventCount", "Top8Count", "FirstPlaceCount" })
            {
                if (ColumnExists("MetaCardSnapshots", column)) continue;

                Create.Column(column).OnTable("MetaCardSnapshots").AsInt32().Nullable().Do();
                Database.Execute($"UPDATE MetaCardSnapshots SET {column} = 0 WHERE {column} IS NULL");
                Alter.Table("MetaCardSnapshots").AlterColumn(column).AsInt32().NotNullable().Do();
            }

            if (!IndexExists("IX_MetaSnapshots_Latest"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_MetaSnapshots_Latest " +
                    "ON dbo.MetaSnapshots (SiteId, FormatId, PeriodId, SnapshotDateUtc) " +
                    "INCLUDE (Id, TotalDecks, IsLatest, CreateDateUtc, LastUpdatedUtc);");
            }

            // Without this the backfill table-scans once per week replayed.
            if (!IndexExists("IX_Tournaments_Site_Period_Date"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_Tournaments_Site_Period_Date " +
                    "ON dbo.Tournaments (SiteId, FormatId, PeriodId, DateUtc) INCLUDE (Id);");
            }

            if (!IndexExists("IX_TournamentEntrants_Tournament"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_TournamentEntrants_Tournament " +
                    "ON dbo.TournamentEntrants (TournamentId) " +
                    "INCLUDE (Id, Placement, Wins, Losses, Draws, TournamentDeckId);");
            }

            if (!IndexExists("IX_DeckCard_Version_Group_Slot"))
            {
                Database.Execute(
                    "CREATE NONCLUSTERED INDEX IX_DeckCard_Version_Group_Slot " +
                    "ON dbo.DeckCard (VersionId, GroupId, SlotId) INCLUDE (CardId);");
            }
        }
    }
}
