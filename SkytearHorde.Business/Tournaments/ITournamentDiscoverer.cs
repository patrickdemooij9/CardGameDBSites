namespace SkytearHorde.Business.Tournaments
{
    /// <summary>
    /// Optional capability implemented by connectors that can discover tournaments on their own (i.e. their
    /// source exposes a searchable overview). Kept separate from <see cref="ITournamentConnector"/> so a source
    /// without a discoverable listing simply doesn't implement it, rather than stubbing a no-op.
    /// </summary>
    public interface ITournamentDiscoverer
    {
        /// <summary>Matches <see cref="ITournamentConnector.Source"/>, e.g. "melee.gg".</summary>
        string Source { get; }

        /// <summary>
        /// Returns the recently-finished tournaments the source can see for the configured game. This should be a
        /// cheap, single listing call — per-tournament work (standings, decklists) is left to the import path.
        /// </summary>
        Task<IReadOnlyList<DiscoveredTournament>> DiscoverTournaments(TournamentDiscoveryConfig config);

        /// <summary>
        /// Cheaply estimates what fraction of a tournament's entrants submitted a decklist, without running a full
        /// import. Returns null when it can't be determined.
        /// </summary>
        Task<DecklistCoverage?> GetDecklistCoverage(string externalId);
    }

    /// <summary>Connector-agnostic slice of per-site config a discoverer needs to build its query.</summary>
    public class TournamentDiscoveryConfig
    {
        /// <summary>Source-specific game identifier to filter on (e.g. melee's "StarWarsUnlimited").</summary>
        public required string GameDescription { get; set; }

        /// <summary>Minimum enrolled players a tournament must have to be worth importing.</summary>
        public int MinPlayers { get; set; }
    }

    /// <summary>Lightweight tournament record produced by discovery, before any import work.</summary>
    public class DiscoveredTournament
    {
        public required string ExternalId { get; set; }
        public required string Name { get; set; }
        public required string GameDescription { get; set; }
        public string Type { get; set; }
        public string? FormatString { get; set; }
        public required string Status { get; set; }
        public DateTime StartDateUtc { get; set; }
        public int EnrolledPlayerCount { get; set; }
        public string? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
    }

    /// <summary>Result of a decklist-coverage probe for a single tournament.</summary>
    public class DecklistCoverage
    {
        public int TotalPlayers { get; set; }
        public int PlayersWithDecklist { get; set; }

        /// <summary>0-100. Zero players yields 0.</summary>
        public double Percent => TotalPlayers == 0 ? 0 : (double)PlayersWithDecklist / TotalPlayers * 100;
    }
}
