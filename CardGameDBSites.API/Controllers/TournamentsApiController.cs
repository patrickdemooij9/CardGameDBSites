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
    public class TournamentsApiController : Controller
    {
        private readonly TournamentService _tournamentService;
        private readonly EntrantService _entrantService;
        private readonly ISiteAccessor _siteAccessor;

        public TournamentsApiController(
            TournamentService tournamentService,
            EntrantService entrantService,
            ISiteAccessor siteAccessor)
        {
            _tournamentService = tournamentService;
            _entrantService = entrantService;
            _siteAccessor = siteAccessor;
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
        public IActionResult Create([FromBody] CreateTournamentDto dto)
        {
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

        [HttpPost("{id:guid}/entrants")]
        public IActionResult AddEntrant(Guid id, [FromBody] AddEntrantDto dto)
        {
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
        public IActionResult UpdateEntrant(Guid entrantId, [FromBody] UpdateEntrantDto dto)
        {
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

        [HttpGet("entrants/{entrantId:guid}")]
        public IActionResult GetEntrant(Guid entrantId)
        {
            var entrant = _entrantService.GetEntrant(entrantId);
            if (entrant is null) return NotFound();
            return Ok(entrant);
        }
    }
}
