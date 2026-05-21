using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Business.Services
{
    public class TournamentMatchService
    {
        private readonly TournamentMatchRepository _matchRepository;
        private readonly TournamentEntrantRepository _entrantRepository;

        public TournamentMatchService(
            TournamentMatchRepository matchRepository,
            TournamentEntrantRepository entrantRepository)
        {
            _matchRepository = matchRepository;
            _entrantRepository = entrantRepository;
        }

        public Guid AddMatch(Guid entrantId, AddTournamentMatchDto dto)
        {
            var entrant = _entrantRepository.Get(entrantId);
            if (entrant is null)
                throw new ArgumentException($"Entrant {entrantId} not found.");

            var match = new TournamentMatch
            {
                Id = Guid.NewGuid(),
                TournamentEventId = entrant.TournamentEventId,
                TournamentEntrantId = entrantId,
                RoundNumber = dto.RoundNumber,
                OpponentName = dto.OpponentName,
                Wins = dto.Wins,
                Losses = dto.Losses,
                Draws = dto.Draws,
            };

            return _matchRepository.Add(match);
        }

        public void UpdateMatch(Guid matchId, UpdateTournamentMatchDto dto)
        {
            var match = _matchRepository.Get(matchId);
            if (match is null)
                throw new ArgumentException($"Match {matchId} not found.");

            match.RoundNumber = dto.RoundNumber;
            match.OpponentName = dto.OpponentName;
            if (dto.Wins.HasValue) match.Wins = dto.Wins.Value;
            if (dto.Losses.HasValue) match.Losses = dto.Losses.Value;
            if (dto.Draws.HasValue) match.Draws = dto.Draws.Value;
            _matchRepository.Update(match);
        }

        public void DeleteMatch(Guid matchId)
        {
            var match = _matchRepository.Get(matchId);
            if (match is null)
                throw new ArgumentException($"Match {matchId} not found.");

            _matchRepository.Delete(matchId);
        }
    }
}
