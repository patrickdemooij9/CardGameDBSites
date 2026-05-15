using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.PostModels;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/tournaments")]
    [IgnoreAntiforgeryToken]
    public class TournamentsApiController : Controller
    {
        private readonly TournamentService _tournamentService;
        private readonly EntrantService _entrantService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly MemberInfoService _memberInfoService;
        private readonly DeckTextParser _deckTextParser;

        public TournamentsApiController(
            TournamentService tournamentService,
            EntrantService entrantService,
            ISiteAccessor siteAccessor,
            MemberInfoService memberInfoService,
            DeckTextParser deckTextParser)
        {
            _tournamentService = tournamentService;
            _entrantService = entrantService;
            _siteAccessor = siteAccessor;
            _memberInfoService = memberInfoService;
            _deckTextParser = deckTextParser;
        }

        [HttpGet]
        public IActionResult List(int? formatId = null)
        {
            var siteId = _siteAccessor.GetSiteId();
            var tournaments = _tournamentService.ListTournaments(siteId, formatId);
            return Ok(tournaments);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var tournament = _tournamentService.GetTournament(id);
            if (tournament is null) return NotFound();
            return Ok(tournament);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Create([FromBody] CreateTournamentDto dto)
        {
            if (!_memberInfoService.IsAdmin())
                return Forbid();

            try
            {
                var id = _tournamentService.CreateTournament(dto);
                return Ok(new { Id = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(Guid id)
        {
            if (!_memberInfoService.IsAdmin())
                return Forbid();

            var tournament = _tournamentService.GetTournament(id);
            if (tournament is null) return NotFound();

            _tournamentService.DeleteTournament(id);
            return Ok();
        }

        [HttpPost("{id:guid}/entrants")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddEntrant(Guid id, [FromBody] AddEntrantDto dto)
        {
            if (!_memberInfoService.IsAdmin())
                return Forbid();

            try
            {
                var entrantId = _entrantService.AddEntrant(id, dto);
                return Ok(new { Id = entrantId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPatch("entrants/{entrantId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateEntrant(Guid entrantId, [FromBody] UpdateEntrantDto dto)
        {
            if (!_memberInfoService.IsAdmin())
                return Forbid();

            try
            {
                _entrantService.UpdateEntrant(entrantId, dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("entrants/{entrantId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteEntrant(Guid entrantId)
        {
            if (!_memberInfoService.IsAdmin())
                return Forbid();

            try
            {
                _entrantService.DeleteEntrant(entrantId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("entrants/{entrantId:guid}")]
        public IActionResult GetEntrant(Guid entrantId)
        {
            var entrant = _entrantService.GetEntrant(entrantId);
            if (entrant is null) return NotFound();
            return Ok(entrant);
        }

        [HttpGet("deck/{deckId:int}")]
        public IActionResult GetByDeck(int deckId)
        {
            var results = _tournamentService.GetTournamentsForDeck(deckId);
            return Ok(results);
        }

        [HttpPost("parse-deck")]
        public IActionResult ParseDeck([FromBody] ParseDeckTextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Text))
                return BadRequest(new { Error = "Text is required." });

            var result = _deckTextParser.Parse(request.Text);
            return Ok(result);
        }
    }
}
