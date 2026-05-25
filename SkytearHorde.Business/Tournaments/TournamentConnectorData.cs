using SkytearHorde.Entities.Models.Business.Tournament;

namespace SkytearHorde.Business.Tournaments
{
    public class TournamentConnectorData
    {
        public List<TournamentMatch> Matches { get; set; } = [];

        /// <summary>
        /// Maps an external entrant/player ID (from the connector source) to its TournamentEntrant instance.
        /// Used to remap match foreign keys to real DB IDs after saving.
        /// </summary>
        public Dictionary<int, TournamentEntrant> EntrantsByExternalId { get; set; } = [];

        /// <summary>
        /// Maps an external round ID (from the connector source) to its TournamentRound instance.
        /// Used to remap match foreign keys to real DB IDs after saving.
        /// </summary>
        public Dictionary<int, TournamentRound> RoundsByExternalId { get; set; } = [];
    }
}
