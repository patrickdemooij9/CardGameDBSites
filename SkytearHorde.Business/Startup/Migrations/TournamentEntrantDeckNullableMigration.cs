using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    /// <summary>
    /// Makes TournamentEntrants.TournamentDeckId nullable so entrants without a decklist (e.g. players
    /// who dropped or never submitted a list but still have match results) can be stored. Matches carry
    /// a foreign key to the entrant, so every entrant must be persisted regardless of whether it has a deck.
    /// </summary>
    public class TournamentEntrantDeckNullableMigration : MigrationBase
    {
        public TournamentEntrantDeckNullableMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (ColumnExists("TournamentEntrants", "TournamentDeckId"))
            {
                Alter.Table("TournamentEntrants").AlterColumn("TournamentDeckId").AsInt32().Nullable().Do();
            }
        }
    }
}
