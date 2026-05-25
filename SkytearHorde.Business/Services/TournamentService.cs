using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Tournaments;
using SkytearHorde.Entities.Models.Business.Tournament;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Business.Services
{
    public class TournamentService
    {
        private readonly TournamentRepository _tournamentRepository;
        private readonly ITournamentConnector[] _tournamentConnectors;

        public TournamentService(TournamentRepository tournamentRepository, IEnumerable<ITournamentConnector> tournamentConnectors)
        {
            _tournamentRepository = tournamentRepository;
            _tournamentConnectors = tournamentConnectors.ToArray();
        }

        public async Task ImportTournament(ImportTournament model)
        {
            var connector = _tournamentConnectors.FirstOrDefault(c => c.Source == model.Source);
            if (connector is null) return;

            var tournamentData = await connector.LoadTournament(model.ExternalId);
            if (tournamentData is null) return;

            var tournament = new Tournament
            {
                Name = tournamentData.Name,
                Type = model.Type,
                FormatId = model.FormatId,
                DateUtc = tournamentData.DateUtc,
                Source = model.Source,
                ExternalUrl = tournamentData.ExternalUrl,
                ExternalId = tournamentData.ExternalId
            };

            _tournamentRepository.Save(tournament);

            var otherData = await connector.GetData(tournament);
            if (otherData is null) return;

            foreach (var round in otherData.RoundsByExternalId.Values)
            {
                _tournamentRepository.Save(round);
            }
            foreach (var entrant in otherData.EntrantsByExternalId.Values)
            {
                _tournamentRepository.Save(entrant);
            }

            // At this point every round and entrant has a real DB Id assigned by Save().
            // Remap the external IDs stored on each match to the actual DB IDs.
            foreach (var match in otherData.Matches)
            {
                if (otherData.RoundsByExternalId.TryGetValue(match.RoundId, out var round))
                    match.RoundId = round.Id;

                if (match.Entrant1Id.HasValue &&
                    otherData.EntrantsByExternalId.TryGetValue(match.Entrant1Id.Value, out var e1))
                    match.Entrant1Id = e1.Id;

                if (match.Entrant2Id.HasValue &&
                    otherData.EntrantsByExternalId.TryGetValue(match.Entrant2Id.Value, out var e2))
                    match.Entrant2Id = e2.Id;

                if (otherData.EntrantsByExternalId.TryGetValue(match.WinnerEntrantId, out var winner))
                    match.WinnerEntrantId = winner.Id;

                _tournamentRepository.Save(match);
            }
        }
    }
}
