using CardGameDBSites.API.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Exports;
using SkytearHorde.Business.Services;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/infographics")]
    [ApiExplorerSettings(GroupName = "Infographics")]
    public class InfographicsController : Controller
    {
        private readonly TournamentService _tournamentService;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly FactService _factService;
        private readonly CardPriceService _cardPriceService;

        public InfographicsController(TournamentService tournamentService,
            CardService cardService,
            SettingsService settingsService,
            IWebHostEnvironment webHostEnvironment,
            FactService factService,
            CardPriceService cardPriceService)
        {
            _tournamentService = tournamentService;
            _cardService = cardService;
            _settingsService = settingsService;
            _webHostEnvironment = webHostEnvironment;
            _factService = factService;
            _cardPriceService = cardPriceService;
        }

        // ---- "Did you know?" facts ----------------------------------------

        [HttpGet("facts")]
        public IActionResult GetFacts([FromQuery] string? setCode = null)
        {
            var result = _factService.GetFacts(setCode).Select(ToApiModel).ToArray();
            return Ok(result);
        }

        [HttpGet("fact/{key}")]
        public async Task<IActionResult> GetFactInfographic(string key, [FromQuery] int slide = 1, [FromQuery] string? setCode = null)
        {
            var parameters = Request.Query
                .Where(q => !q.Key.Equals("slide", StringComparison.OrdinalIgnoreCase)
                         && !q.Key.Equals("setCode", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(q => q.Key, q => q.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var fact = _factService.GetFact(key, parameters, setCode);
            if (fact is null) return NotFound();

            var data = MapFact(fact);
            var slideCount = FactInfographicExport.SlideCount(data);
            if (slide < 1 || slide > slideCount) return BadRequest($"slide must be between 1 and {slideCount}");

            var exporter = new FactInfographicExport(_webHostEnvironment);
            var bytes = await exporter.Render(data, slide);
            return File(bytes, "image/png");
        }

        private FactInfographicData MapFact(SkytearHorde.Business.Facts.GameFact fact)
        {
            return new FactInfographicData
            {
                Hook = fact.Hook,
                FooterText = GetFooterText(),
                Slides = fact.Slides.Select(s => new InfographicFactSlide
                {
                    Kind = s.Kind,
                    Heading = s.Heading,
                    Title = s.Title,
                    BigValue = s.BigValue,
                    BigLabel = s.BigLabel,
                    Caption = s.Caption,
                    ImageUrl = ImageCropHelper.ToAbsolute(s.ImageUrl),
                    Items = s.Items
                }).ToArray()
            };
        }

        private static object ToApiModel(SkytearHorde.Business.Facts.GameFact fact) => new
        {
            key = fact.Key,
            hook = fact.Hook,
            slides = fact.Slides.Select(s => new
            {
                kind = s.Kind.ToString(),
                heading = s.Heading,
                title = s.Title,
                bigValue = s.BigValue,
                bigLabel = s.BigLabel,
                caption = s.Caption,
                imageUrl = ImageCropHelper.ToAbsolute(s.ImageUrl),
                items = s.Items
            })
        };

        // ---- Weekly price trends ------------------------------------------

        [HttpGet("price-trends")]
        public async Task<IActionResult> GetPriceTrends([FromQuery] int slide = 1)
        {
            if (slide < 1 || slide > PriceTrendInfographicExport.SlideCount)
                return BadRequest($"slide must be between 1 and {PriceTrendInfographicExport.SlideCount}");

            if (!_settingsService.GetSiteSettings().AllowPricing)
                return NotFound();

            var end = (_cardPriceService.GetLatestPriceDate() ?? DateTime.UtcNow).Date;
            var data = new PriceTrendInfographicData
            {
                DateRangeLabel = $"{end.AddDays(-7):MMM d} – {end:MMM d}",
                Risers = BuildMovers(descending: true),
                Fallers = BuildMovers(descending: false),
                FooterText = GetFooterText()
            };

            var exporter = new PriceTrendInfographicExport(_webHostEnvironment);
            var bytes = await exporter.Render(data, slide);
            return File(bytes, "image/png");
        }

        private PriceTrendEntry[] BuildMovers(bool descending)
        {
            // Small buffer over the 5 we show, so the site filter below can still fill 5.
            var changes = _cardPriceService.GetTopWeeklyPriceChanges(15, descending);
            var cards = _cardService.Get(changes.Select(c => c.CardId).Distinct().ToArray())
                .ToDictionary(c => c.BaseId);

            return changes
                .Where(c => cards.ContainsKey(c.CardId))
                .Take(5)
                .Select((c, index) =>
                {
                    var card = cards[c.CardId];
                    return new PriceTrendEntry
                    {
                        Rank = index + 1,
                        Name = card.DisplayName ?? "Unknown",
                        ImageUrl = card.Image is null ? null : ImageCropHelper.ToApiModels(card.Image).Url,
                        OldPrice = c.PreviousPrice,
                        NewPrice = c.CurrentPrice
                    };
                })
                .ToArray();
        }

        [HttpGet("tournament/{id}")]
        public async Task<IActionResult> GetTournamentInfographic(int id, [FromQuery] int slide = 1)
        {
            if (slide is < 1 or > 4) return BadRequest("slide must be between 1 and 4");

            var tournament = _tournamentService.GetById(id);
            if (tournament is null) return NotFound();

            var data = new TournamentInfographicData
            {
                TournamentName = tournament.Name,
                DateUtc = tournament.DateUtc,
                PlayerCount = _tournamentService.GetPlayerCount(id),
                Entries = [],
                FooterText = GetFooterText()
            };

            var exporter = new TournamentInfographicExport(_webHostEnvironment);
            byte[] bytes;
            switch (slide)
            {
                case 1:
                    bytes = await exporter.RenderHookSlide(data);
                    break;
                case 2:
                    data.Entries = BuildTop8Entries(id);
                    bytes = await exporter.RenderTop8Slide(data);
                    break;
                case 3:
                    data.Aspects = _tournamentService.GetAspectRepresentation(id)
                        .Select(a => new InfographicAspect { Name = a.Name, Percentage = a.Percentage })
                        .ToArray();
                    bytes = await exporter.RenderAspectsSlide(data);
                    break;
                default: // 4
                    data.TopCards = BuildTopCards(id, 9);
                    bytes = await exporter.RenderCardsSlide(data);
                    break;
            }

            return File(bytes, "image/png");
        }

        private TournamentInfographicEntry[] BuildTop8Entries(int tournamentId)
        {
            return _tournamentService.GetTop8Entrants(tournamentId)
                .OrderBy(e => e.Placement)
                .Select(e =>
                {
                    var champion = e.LeaderCardId.HasValue ? _cardService.Get(e.LeaderCardId.Value) : null;
                    return new TournamentInfographicEntry
                    {
                        Placement = e.Placement,
                        PlayerName = e.PlayerName,
                        DeckName = e.DeckName,
                        ChampionName = champion?.DisplayName,
                        ChampionImageUrl = champion?.Image is null ? null : ImageCropHelper.ToApiModels(champion.Image).Url
                    };
                })
                .ToArray();
        }

        private InfographicCard[] BuildTopCards(int tournamentId, int take)
        {
            return _tournamentService.GetMostPlayedCards(tournamentId, take)
                .Select(c =>
                {
                    var card = _cardService.Get(c.CardId);
                    return new InfographicCard
                    {
                        Name = card?.DisplayName ?? "Unknown",
                        ImageUrl = card?.Image is null ? null : ImageCropHelper.ToApiModels(card.Image).Url,
                        Percentage = c.Percentage
                    };
                })
                .ToArray();
        }

        private string? GetFooterText()
        {
            try
            {
                var baseUrl = _settingsService.GetSiteSettings().BaseUrl;
                return baseUrl?.Replace("https://", "").Replace("http://", "").TrimEnd('/');
            }
            catch
            {
                return null;
            }
        }
    }
}
