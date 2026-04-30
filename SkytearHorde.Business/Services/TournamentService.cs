using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;

namespace SkytearHorde.Business.Services
{
    public class TournamentService
    {
        private readonly TournamentRepository _tournamentRepository;
        private readonly TournamentEntrantRepository _entrantRepository;
        private readonly SettingsService _settingsService;
        private readonly ISiteAccessor _siteAccessor;

        public TournamentService(
            TournamentRepository tournamentRepository,
            TournamentEntrantRepository entrantRepository,
            SettingsService settingsService,
            ISiteAccessor siteAccessor)
        {
            _tournamentRepository = tournamentRepository;
            _entrantRepository = entrantRepository;
            _settingsService = settingsService;
            _siteAccessor = siteAccessor;
        }

        public Guid CreateTournament(CreateTournamentDto dto)
        {
            var siteId = _siteAccessor.GetSiteId();
            var types = _settingsService.GetTypes();

            if (!types.Any(t => t.Id == dto.FormatId))
                throw new ArgumentException($"FormatId {dto.FormatId} is not a valid squad settings TypeID for this site.");

            var tournament = new TournamentEvent
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                Name = dto.Name,
                Date = dto.Date,
                FormatId = dto.FormatId,
                PlayerCount = dto.PlayerCount,
                SourceUrl = dto.SourceUrl
            };

            return _tournamentRepository.Create(tournament);
        }

        public TournamentEvent? GetTournament(Guid id)
        {
            var tournament = _tournamentRepository.Get(id);
            if (tournament is null) return null;

            var types = _settingsService.GetTypes();
            var format = types.FirstOrDefault(t => t.Id == tournament.FormatId);
            tournament.FormatDisplayName = format?.DisplayName;
            tournament.Entrants = _entrantRepository.GetByTournament(id);

            return tournament;
        }

        public List<TournamentEvent> ListTournaments(int siteId, int? formatId = null)
        {
            var tournaments = _tournamentRepository.GetBySite(siteId, formatId);
            var types = _settingsService.GetTypes();

            foreach (var t in tournaments)
            {
                var format = types.FirstOrDefault(ty => ty.Id == t.FormatId);
                t.FormatDisplayName = format?.DisplayName;
            }

            return tournaments;
        }
    }
}
