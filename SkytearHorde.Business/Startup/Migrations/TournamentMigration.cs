using SkytearHorde.Entities.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class TournamentMigration : MigrationBase
    {
        public TournamentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("TournamentEvent"))
            {
                Create.Table<TournamentEventDBModel>().Do();
            }

            if (!TableExists("TournamentEntrant"))
            {
                Create.Table<TournamentEntrantDBModel>().Do();
            }

            if (!TableExists("Archetype"))
            {
                Create.Table<ArchetypeDBModel>().Do();
            }

            if (!TableExists("DeckArchetype"))
            {
                Create.Table<DeckArchetypeDBModel>().Do();
            }

            if (!IndexExists("IX_TournamentEvent_SiteId_FormatId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_TournamentEvent_SiteId_FormatId ON TournamentEvent (SiteId, FormatId)");
            }

            if (!IndexExists("IX_TournamentEntrant_TournamentEventId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_TournamentEntrant_TournamentEventId ON TournamentEntrant (TournamentEventId)");
            }

            if (!IndexExists("IX_Archetype_SiteId_FormatId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_Archetype_SiteId_FormatId ON Archetype (SiteId, FormatId)");
            }

            if (!IndexExists("IX_DeckArchetype_ArchetypeId"))
            {
                Database.Execute("CREATE NONCLUSTERED INDEX IX_DeckArchetype_ArchetypeId ON DeckArchetype (ArchetypeId)");
            }
        }
    }
}
