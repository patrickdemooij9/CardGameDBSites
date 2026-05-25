using SkytearHorde.Entities.Models.Business.Tournament;

namespace SkytearHorde.Business.Tournaments
{
    public interface ITournamentConnector
    {
        string Source { get; }

        Task<Tournament?> LoadTournament(string externalId);
        Task<TournamentConnectorData?> GetData(Tournament tournament);
    }
}
