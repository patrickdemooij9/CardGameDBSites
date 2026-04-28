using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.DailyGame;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Security;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/dailygame")]
    [OptionalJwtAuthorization]
    public class DailyGameApiController : Controller
    {
        private readonly DailyGameService _dailyGameService;
        private readonly IMemberManager _memberManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DailyGameApiController(DailyGameService dailyGameService, IMemberManager memberManager, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _dailyGameService = dailyGameService;
            _memberManager = memberManager;
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("bootstrap")]
        [ProducesResponseType(typeof(DailyGameBootstrapApiModel), 200)]
        public async Task<IActionResult> Bootstrap(string? guestSessionToken = null)
        {
            var memberId = await GetCurrentMemberId();
            var session = _dailyGameService.GetSession(memberId, guestSessionToken);
            var leaderboard = session.IsFinished ? _dailyGameService.GetLeaderboard(50, memberId) : [];
            var placement = _dailyGameService.GetPlacement(session, session.ChallengeId, memberId);

            return Ok(MapBootstrap(session, leaderboard, placement));
        }

        [HttpPost("guess")]
        [ProducesResponseType(typeof(DailyGameGuessResultApiModel), 200)]
        public async Task<IActionResult> Guess(DailyGameGuessPostApiModel model)
        {
            var memberId = await GetCurrentMemberId();
            var result = _dailyGameService.SubmitGuess(model.GuessedCardId, memberId, model.GuestSessionToken);
            var leaderboard = result.State.IsFinished ? _dailyGameService.GetLeaderboard(50, memberId) : [];
            var bootstrap = MapBootstrap(result.State, leaderboard, result.CurrentPlacement);

            return Ok(new DailyGameGuessResultApiModel
            {
                State = bootstrap,
                LatestAttempt = result.LatestAttempt is null ? null : MapAttempt(result.LatestAttempt)
            });
        }

        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(DailyGameLeaderboardEntryApiModel[]), 200)]
        public async Task<IActionResult> Leaderboard(int take = 50)
        {
            var memberId = await GetCurrentMemberId();
            var result = _dailyGameService.GetLeaderboard(Math.Clamp(take, 1, 200), memberId)
                .Select(MapLeaderboard)
                .ToArray();
            return Ok(result);
        }

        [HttpGet("image")]
        public async Task<IActionResult> Image(string? guestSessionToken = null)
        {
            var imageUrl = _dailyGameService.GetTargetImageUrl();
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return NotFound();
            }

            var memberId = await GetCurrentMemberId();
            var session = _dailyGameService.GetSession(memberId, guestSessionToken);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{imageUrl}");
            using var image = SixLabors.ImageSharp.Image.Load(path);
            if (!session.IsFinished)
            {
                image.Mutate(it => it.GaussianBlur(_dailyGameService.GetBlurLevel(session.AttemptsUsed)));
            }

            using var stream = new MemoryStream();
            await image.SaveAsync(stream, image.DetectEncoder(path));
            stream.Position = 0;
            return File(stream.ToArray(), "image/webp");
        }

        private async Task<int?> GetCurrentMemberId()
        {
            var member = await _memberManager.GetCurrentMemberAsync();
            if (member == null || !int.TryParse(member.Id, out var memberId))
            {
                return null;
            }
            return memberId;
        }

        private DailyGameBootstrapApiModel MapBootstrap(DailyGameSessionState session, DailyGameLeaderboardEntryState[] leaderboard, DailyGameLeaderboardEntryState? placement)
        {
            var now = session.FinishedUtc ?? DateTime.UtcNow;
            var elapsed = Math.Max(0, (int)(now - session.StartedUtc).TotalSeconds);

            return new DailyGameBootstrapApiModel
            {
                GuestSessionToken = session.GuestSessionToken,
                MaxAttempts = session.MaxAttempts,
                AttemptsUsed = session.AttemptsUsed,
                AttemptsLeft = Math.Max(0, session.MaxAttempts - session.AttemptsUsed),
                ElapsedSeconds = elapsed,
                BlurLevel = _dailyGameService.GetBlurLevel(session.AttemptsUsed),
                IsFinished = session.IsFinished,
                IsSolved = session.IsSolved,
                Attempts = session.Attempts.Select(MapAttempt).ToArray(),
                Leaderboard = leaderboard.Select(MapLeaderboard).ToArray(),
                CurrentPlacement = placement is null ? null : MapLeaderboard(placement)
            };
        }

        private static DailyGameAttemptApiModel MapAttempt(DailyGameAttemptState attempt)
        {
            return new DailyGameAttemptApiModel
            {
                AttemptNumber = attempt.AttemptNumber,
                GuessedCardId = attempt.GuessedCardId,
                GuessedCardName = attempt.GuessedCardName,
                IsCorrect = attempt.IsCorrect,
                CreatedUtc = attempt.CreatedUtc,
                Feedback = attempt.Feedback.Select(it => new DailyGameAttributeFeedbackApiModel
                {
                    Name = it.Name,
                    MatchType = it.MatchType,
                    GuessValues = it.GuessValues
                }).ToArray()
            };
        }

        private static DailyGameLeaderboardEntryApiModel MapLeaderboard(DailyGameLeaderboardEntryState entry)
        {
            return new DailyGameLeaderboardEntryApiModel
            {
                Rank = entry.Rank,
                MemberId = entry.MemberId,
                MemberName = entry.MemberName,
                AttemptsUsed = entry.AttemptsUsed,
                ElapsedSeconds = entry.ElapsedSeconds,
                Solved = entry.Solved,
                IsCurrentPlayer = entry.IsCurrentPlayer
            };
        }
    }
}
