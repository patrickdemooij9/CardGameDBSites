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

        [HttpGet("recent")]
        [ProducesResponseType(typeof(TournamentSummaryApiModel[]), 200)]
        public IActionResult GetRecent([FromQuery] int count = 6)
        {
            var tournaments = _tournamentService.GetRecent(count);

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

        [HttpGet("{id}/top8")]
        [ProducesResponseType(typeof(TournamentEntrantApiModel[]), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetTop8(int id)
        {
            var top8 = _tournamentService.GetTop8Entrants(id)
                .Select(e => new TournamentEntrantApiModel
                {
                    Id = e.Id,
                    PlayerName = e.PlayerName,
                    Placement = e.Placement,
                    DeckId = e.TournamentDeckId,
                    DeckName = e.DeckName
                })
                .ToArray();

            return Ok(top8);
        }
    }
}
