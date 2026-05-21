using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Business.Services
{
    public class EntrantService
    {
        private readonly TournamentEntrantRepository _entrantRepository;
        private readonly TournamentRepository _tournamentRepository;
        private readonly TournamentMatchRepository _matchRepository;

        public EntrantService(
            TournamentEntrantRepository entrantRepository,
            TournamentRepository tournamentRepository,
            TournamentMatchRepository matchRepository)
        {
            _entrantRepository = entrantRepository;
            _tournamentRepository = tournamentRepository;
            _matchRepository = matchRepository;
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
                DeckId = dto.DeckId
            };

            var entrantId = _entrantRepository.Add(entrant);
            foreach (var match in dto.Matches ?? [])
            {
                _matchRepository.Add(new TournamentMatch
                {
                    Id = Guid.NewGuid(),
                    TournamentEventId = tournamentId,
                    TournamentEntrantId = entrantId,
                    RoundNumber = match.RoundNumber,
                    OpponentName = match.OpponentName,
                    Wins = match.Wins,
                    Losses = match.Losses,
                    Draws = match.Draws,
                });
            }

            return entrantId;
        }

        public void UpdateEntrant(Guid entrantId, UpdateEntrantDto dto)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null)
                throw new ArgumentException($"Entrant {entrantId} not found.");

            if (dto.PlayerName is not null) entrant.PlayerName = dto.PlayerName;
            if (dto.Placement.HasValue) entrant.Placement = dto.Placement;
            if (dto.DeckId.HasValue) entrant.DeckId = dto.DeckId;

            _entrantRepository.Update(entrant);
        }

        public void DeleteEntrant(Guid entrantId)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null)
                throw new ArgumentException($"Entrant {entrantId} not found.");

            _matchRepository.DeleteByEntrant(entrantId);
            _entrantRepository.Delete(entrantId);
        }

        public TournamentEntrant? GetEntrant(Guid entrantId)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null) return null;

            entrant.Matches = _matchRepository.GetByEntrant(entrantId)
                .OrderBy(m => m.RoundNumber ?? int.MaxValue)
                .ThenBy(m => m.CreatedAt)
                .ToList();
            if (entrant.Matches.Count > 0)
            {
                entrant.Wins = entrant.Matches.Sum(m => m.Wins);
                entrant.Losses = entrant.Matches.Sum(m => m.Losses);
                entrant.Draws = entrant.Matches.Sum(m => m.Draws);
            }
            return entrant;
        }
    }
}
