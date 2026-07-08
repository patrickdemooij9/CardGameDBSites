using CardGameDBSites.API.Models.Tournaments;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/tournaments")]
    [ApiExplorerSettings(GroupName = "Tournaments")]
    public class TournamentApiController : Controller
    {
        private readonly TournamentService _tournamentService;

        public TournamentApiController(TournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        [HttpGet("periods")]
        [ProducesResponseType(typeof(PeriodApiModel[]), 200)]
        public IActionResult GetPeriods([FromQuery] int formatId)
        {
            var periods = _tournamentService.GetPeriods(formatId).ToArray();
            var currentId = _tournamentService.GetCurrentPeriod(formatId)?.Id;

            var result = periods.Select(p => new PeriodApiModel
            {
                Id = p.Id,
                Name = p.Name,
                StartingDateUtc = p.StartingDateUtc,
                EndDateUtc = p.EndDateUtc,
                IsCurrent = p.Id == currentId
            }).ToArray();

            return Ok(result);
        }

        [HttpGet("recent")]
        [ProducesResponseType(typeof(TournamentSummaryApiModel[]), 200)]
        public IActionResult GetRecent([FromQuery] int periodId, [FromQuery] int count = 6)
        {
            var tournaments = _tournamentService.GetRecent(periodId, count);

            var result = tournaments.Select(t =>
            {
                var playerCount = _tournamentService.GetPlayerCount(t.Id);
                var top8 = _tournamentService.GetTop8Entrants(t.Id).ToArray();
                var winner = top8.FirstOrDefault(e => e.Placement == 1);

                return new TournamentSummaryApiModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    DateUtc = t.DateUtc,
                    Type = t.Type,
                    ExternalUrl = t.ExternalUrl,
                    PlayerCount = playerCount,
                    Winner = winner is null ? null : new TournamentEntrantApiModel
                    {
                        Id = winner.Id,
                        PlayerName = winner.PlayerName,
                        Placement = winner.Placement,
                        DeckId = winner.TournamentDeckId,
                        DeckName = winner.DeckName
                    }
                };
            }).ToArray();

            return Ok(result);
        }

        [HttpGet("meta/recent-winners")]
        [ProducesResponseType(typeof(MetaWinningDeckApiModel[]), 200)]
        public IActionResult GetRecentWinners([FromQuery] int periodId, [FromQuery] int count = 6, [FromQuery] int leaderGroupId = 1, [FromQuery] int leaderSlotId = 0)
        {
            var result = _tournamentService.GetRecentWinningDecks(periodId, count, leaderGroupId, leaderSlotId)
                .Select(d => new MetaWinningDeckApiModel
                {
                    TournamentId = d.TournamentId,
                    TournamentName = d.TournamentName,
                    TournamentDateUtc = d.TournamentDateUtc,
                    ExternalUrl = d.ExternalUrl,
                    PlayerName = d.PlayerName,
                    DeckId = d.DeckId,
                    DeckName = d.DeckName,
                    LeaderName = d.LeaderName
                })
                .ToArray();

            return Ok(result);
        }

        [HttpGet("meta/top-leaders")]
        [ProducesResponseType(typeof(MetaLeaderApiModel[]), 200)]
        public IActionResult GetTopLeaders([FromQuery] int periodId, [FromQuery] int take = 5, [FromQuery] int leaderGroupId = 1, [FromQuery] int leaderSlotId = 0, [FromQuery] int? tournamentId = null)
        {
            var result = _tournamentService.GetTopLeaders(periodId, take, leaderGroupId, leaderSlotId, tournamentId)
                .Select(l => new MetaLeaderApiModel
                {
                    LeaderName = l.LeaderName,
                    Wins = l.Wins,
                    Top8Count = l.Top8Count
                })
                .ToArray();

            return Ok(result);
        }

        [HttpGet("meta/popular-cards")]
        [ProducesResponseType(typeof(MetaPopularCardApiModel[]), 200)]
        public IActionResult GetPopularCards([FromQuery] int periodId, [FromQuery] int take = 8, [FromQuery] int leaderGroupId = 1, [FromQuery] int leaderSlotId = 0)
        {
            var result = _tournamentService.GetPopularCards(periodId, take, leaderGroupId, leaderSlotId)
                .Select(c => new MetaPopularCardApiModel
                {
                    CardName = c.CardName,
                    Percentage = c.Percentage
                })
                .ToArray();

            return Ok(result);
        }
    }
}
