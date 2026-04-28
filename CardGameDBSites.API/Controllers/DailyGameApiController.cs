using CardGameDBSites.API.Attributes;
using CardGameDBSites.API.Models.DailyGame;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DailyGameApiController(DailyGameService dailyGameService, IMemberManager memberManager, IWebHostEnvironment webHostEnvironment)
        {
            _dailyGameService = dailyGameService;
            _memberManager = memberManager;
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
            var imageDataUrl = await GetBlurredImageDataUrlAsync(session.AttemptsUsed, session.IsFinished);

            return Ok(await MapBootstrap(session, leaderboard, placement, imageDataUrl));
        }

        [HttpPost("guess")]
        [ProducesResponseType(typeof(DailyGameGuessResultApiModel), 200)]
        public async Task<IActionResult> Guess(DailyGameGuessPostApiModel model)
        {
            var memberId = await GetCurrentMemberId();
            var result = _dailyGameService.SubmitGuess(model.GuessedCardId, memberId, model.GuestSessionToken);
            var leaderboard = result.State.IsFinished ? _dailyGameService.GetLeaderboard(50, memberId) : [];
            var imageDataUrl = await GetBlurredImageDataUrlAsync(result.State.AttemptsUsed, result.State.IsFinished);
            var bootstrap = await MapBootstrap(result.State, leaderboard, result.CurrentPlacement, imageDataUrl);

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

        private async Task<string?> GetBlurredImageDataUrlAsync(int attemptsUsed, bool isFinished)
        {
            var imageUrl = _dailyGameService.GetTargetImageUrl();
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{imageUrl}");
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(path);
                if (!isFinished)
                {
                    image.Mutate(it => it.GaussianBlur(_dailyGameService.GetBlurLevel(attemptsUsed)));
                }

                var provider = new FileExtensionContentTypeProvider();

                if (!provider.TryGetContentType(path, out var contentType))
                {
                    contentType = "image/png";
                }

                using var stream = new MemoryStream();
                await image.SaveAsync(stream, image.DetectEncoder(path));
                var base64 = Convert.ToBase64String(stream.ToArray());
                return $"data:{contentType};base64,{base64}";
            }
            catch
            {
                return null;
            }
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

        private async Task<DailyGameBootstrapApiModel> MapBootstrap(DailyGameSessionState session, DailyGameLeaderboardEntryState[] leaderboard, DailyGameLeaderboardEntryState? placement, string? imageDataUrl)
        {
            var now = session.FinishedUtc ?? DateTime.UtcNow;
            var elapsed = Math.Max(0, (int)(now - session.StartedUtc).TotalSeconds);

            // Include the guest's simulated placement in the leaderboard list so the frontend
            // can render it without needing to special-case it outside the table.
            var leaderboardEntries = leaderboard.Select(MapLeaderboard).ToList();
            if (placement is { IsCurrentPlayer: true } && leaderboardEntries.All(e => !e.IsCurrentPlayer))
            {
                var mapped = MapLeaderboard(placement);
                var insertIndex = Math.Clamp(mapped.Rank - 1, 0, leaderboardEntries.Count);
                leaderboardEntries.Insert(insertIndex, mapped);
            }

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
                ImageDataUrl = imageDataUrl,
                Attempts = session.Attempts.Select(MapAttempt).ToArray(),
                Leaderboard = leaderboardEntries.ToArray(),
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
