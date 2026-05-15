using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Business.Services
{
    public class EntrantService
    {
        private readonly TournamentEntrantRepository _entrantRepository;
        private readonly TournamentRepository _tournamentRepository;

        public EntrantService(TournamentEntrantRepository entrantRepository, TournamentRepository tournamentRepository)
        {
            _entrantRepository = entrantRepository;
            _tournamentRepository = tournamentRepository;
        }

        public Guid AddEntrant(Guid tournamentId, AddEntrantDto dto)
        {
            var tournament = _tournamentRepository.Get(tournamentId);
            if (tournament is null)
                throw new ArgumentException($"Tournament {tournamentId} not found.");

            var entrant = new TournamentEntrant
            {
                Id = Guid.NewGuid(),
                TournamentEventId = tournamentId,
                PlayerName = dto.PlayerName,
                Placement = dto.Placement,
                Wins = dto.Wins,
                Losses = dto.Losses,
                Draws = dto.Draws,
                DeckId = dto.DeckId
            };

            return _entrantRepository.Add(entrant);
        }

        public void UpdateEntrant(Guid entrantId, UpdateEntrantDto dto)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null)
                throw new ArgumentException($"Entrant {entrantId} not found.");

            if (dto.PlayerName is not null) entrant.PlayerName = dto.PlayerName;
            if (dto.Placement.HasValue) entrant.Placement = dto.Placement;
            if (dto.Wins.HasValue) entrant.Wins = dto.Wins;
            if (dto.Losses.HasValue) entrant.Losses = dto.Losses;
            if (dto.Draws.HasValue) entrant.Draws = dto.Draws;
            if (dto.DeckId.HasValue) entrant.DeckId = dto.DeckId;

            _entrantRepository.Update(entrant);
        }

        public void DeleteEntrant(Guid entrantId)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null)
                throw new ArgumentException($"Entrant {entrantId} not found.");

            _entrantRepository.Delete(entrantId);
        }

        public TournamentEntrant? GetEntrant(Guid entrantId)
        {
            return _entrantRepository.Get(entrantId);
        }
    }
}
