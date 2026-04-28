using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Database.DailyGame;
using System.Collections.Concurrent;
using System.Text.Json;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class DailyGameService
    {
        private const int MaxAttempts = 5;
        private readonly DailyCardChallengeRepository _challengeRepository;
        private readonly DailyCardGameSessionRepository _sessionRepository;
        private readonly DailyCardGameGuessRepository _guessRepository;
        private readonly CardService _cardService;
        private readonly MemberInfoService _memberInfoService;
        private static readonly ConcurrentDictionary<string, DailyGameSessionState> GuestSessions = new();

        public DailyGameService(
            DailyCardChallengeRepository challengeRepository,
            DailyCardGameSessionRepository sessionRepository,
            DailyCardGameGuessRepository guessRepository,
            CardService cardService,
            MemberInfoService memberInfoService)
        {
            _challengeRepository = challengeRepository;
            _sessionRepository = sessionRepository;
            _guessRepository = guessRepository;
            _cardService = cardService;
            _memberInfoService = memberInfoService;
        }

        public DailyCardChallengeDBModel GetOrCreateTodayChallenge()
        {
            var today = DateTime.UtcNow.Date;
            var existing = _challengeRepository.GetByDate(today);
            if (existing != null)
            {
                return existing;
            }

            var validSets = _cardService.GetAllSets().Where(it => it.HasBeenReleased);
            var allCards = validSets
                .SelectMany(it => _cardService.GetAllBySet(it.Id, true))
                .Where(it => !it.HideFromDecks && it.Image != null && it.VariantId > 0 && it.VariantTypeId == null)
                .ToArray();
            if (allCards.Length == 0)
            {
                throw new InvalidOperationException("No valid cards available for the daily challenge.");
            }

            var selectedCard = allCards[Random.Shared.Next(0, allCards.Length)];
            var result = _challengeRepository.Add(selectedCard.VariantId, today);
            GuestSessions.Clear();
            return result;
        }

        public DailyGameSessionState GetSession(int? memberId, string? guestSessionToken)
        {
            var challenge = GetOrCreateTodayChallenge();
            if (memberId.HasValue)
            {
                return GetOrCreateMemberSession(challenge, memberId.Value);
            }

            return GetOrCreateGuestSession(challenge, guestSessionToken);
        }

        public DailyGameGuessState SubmitGuess(int guessedCardId, int? memberId, string? guestSessionToken)
        {
            var session = GetSession(memberId, guestSessionToken);
            if (session.IsFinished)
            {
                throw new InvalidOperationException("Game is already finished for this session.");
            }

            if (session.AttemptsUsed >= session.MaxAttempts)
            {
                throw new InvalidOperationException("No attempts left.");
            }

            var challenge = GetOrCreateTodayChallenge();
            var targetCard = _cardService.GetVariant(challenge.TargetCardId) ?? throw new InvalidOperationException("Target card not found.");
            var guessedCard = _cardService.GetVariant(guessedCardId) ?? throw new InvalidOperationException("Guessed card not found.");

            var feedback = BuildFeedback(targetCard, guessedCard);
            var isCorrect = guessedCard.VariantId == targetCard.VariantId;
            var now = DateTime.UtcNow;
            var nextAttemptNumber = session.AttemptsUsed + 1;
            var attempt = new DailyGameAttemptState
            {
                AttemptNumber = nextAttemptNumber,
                GuessedCardId = guessedCard.VariantId,
                GuessedCardName = guessedCard.DisplayName,
                IsCorrect = isCorrect,
                Feedback = feedback,
                CreatedUtc = now
            };

            session.Attempts = [.. session.Attempts, attempt];
            session.AttemptsUsed++;

            session.Status = DailyGameRules.ResolveStatus(isCorrect, session.AttemptsUsed, session.MaxAttempts);
            if (session.Status == "Solved")
            {
                session.IsSolved = true;
                session.IsFinished = true;
                session.FinishedUtc = now;
            }
            else if (session.Status == "Failed")
            {
                session.IsFinished = true;
                session.FinishedUtc = now;
            }

            if (memberId.HasValue)
            {
                PersistMemberSession(session, challenge, memberId.Value, attempt);
            }
            else
            {
                StoreGuestSession(session);
            }

            var currentPlacement = GetPlacement(session, challenge.Id, memberId);
            return new DailyGameGuessState
            {
                State = session,
                LatestAttempt = attempt,
                CurrentPlacement = currentPlacement
            };
        }

        public DailyGameLeaderboardEntryState[] GetLeaderboard(int take, int? currentMemberId = null)
        {
            var challenge = GetOrCreateTodayChallenge();
            var completed = DailyGameRules
                .Rank(
                    _sessionRepository.GetCompletedByChallenge(challenge.Id),
                    it => it.Solved,
                    it => it.AttemptsUsed,
                    it => GetElapsedSeconds(it.StartedUtc, it.FinishedUtc ?? DateTime.UtcNow))
                .Take(take)
                .ToArray();

            return completed.Select((entry, index) => new DailyGameLeaderboardEntryState
            {
                Rank = index + 1,
                MemberId = entry.MemberId,
                MemberName = entry.MemberId.HasValue ? _memberInfoService.Get(entry.MemberId.Value)?.DisplayName : null,
                AttemptsUsed = entry.AttemptsUsed,
                ElapsedSeconds = GetElapsedSeconds(entry.StartedUtc, entry.FinishedUtc ?? DateTime.UtcNow),
                Solved = entry.Solved,
                IsCurrentPlayer = currentMemberId.HasValue && entry.MemberId == currentMemberId,
            }).ToArray();
        }

        public DailyGameLeaderboardEntryState? GetPlacement(DailyGameSessionState state, int challengeId, int? currentMemberId)
        {
            if (!state.IsFinished)
            {
                return null;
            }

            var leaderboard = GetLeaderboard(int.MaxValue, currentMemberId).ToList();
            if (currentMemberId.HasValue)
            {
                return leaderboard.FirstOrDefault(it => it.MemberId == currentMemberId);
            }

            var simulated = new DailyGameLeaderboardEntryState
            {
                MemberId = null,
                MemberName = "Guest",
                AttemptsUsed = state.AttemptsUsed,
                ElapsedSeconds = GetElapsedSeconds(state.StartedUtc, state.FinishedUtc ?? DateTime.UtcNow),
                Solved = state.IsSolved,
                IsCurrentPlayer = true,
            };

            leaderboard.Add(simulated);
            var ordered = DailyGameRules
                .Rank(leaderboard, it => it.Solved, it => it.AttemptsUsed, it => it.ElapsedSeconds)
                .ToArray();

            var index = Array.FindIndex(ordered, it => ReferenceEquals(it, simulated));
            simulated.Rank = index + 1;
            return simulated;
        }

        public int GetBlurLevel(int attemptsUsed)
        {
            return attemptsUsed switch
            {
                <= 0 => 30,
                1 => 25,
                2 => 20,
                3 => 15,
                4 => 10,
                _ => 0
            };
        }

        public string? GetTargetImageUrl()
        {
            var challenge = GetOrCreateTodayChallenge();
            return _cardService.GetVariant(challenge.TargetCardId)?.Image?.Url();
        }

        private DailyGameSessionState GetOrCreateMemberSession(DailyCardChallengeDBModel challenge, int memberId)
        {
            var session = _sessionRepository.GetByChallengeAndMember(challenge.Id, memberId);
            if (session == null)
            {
                var created = new DailyCardGameSessionDBModel
                {
                    ChallengeId = challenge.Id,
                    SiteId = challenge.SiteId,
                    MemberId = memberId,
                    Status = "InProgress",
                    AttemptsUsed = 0,
                    MaxAttempts = MaxAttempts,
                    StartedUtc = DateTime.UtcNow,
                    Solved = false
                };
                _sessionRepository.Add(created);
                return MapSession(created, []);
            }

            var attempts = _guessRepository.GetBySession(session.Id)
                .Select(it => MapAttempt(it, challenge.TargetCardId))
                .ToArray();
            return MapSession(session, attempts);
        }

        private DailyGameSessionState GetOrCreateGuestSession(DailyCardChallengeDBModel challenge, string? guestSessionToken)
        {
            if (!string.IsNullOrWhiteSpace(guestSessionToken))
            {
                if (GuestSessions.TryGetValue(GetGuestCacheKey(guestSessionToken), out var existing) && existing.ChallengeId == challenge.Id)
                {
                    return existing;
                }
            }

            var token = Guid.NewGuid().ToString("N");
            var state = new DailyGameSessionState
            {
                ChallengeId = challenge.Id,
                GuestSessionToken = token,
                Status = "InProgress",
                AttemptsUsed = 0,
                MaxAttempts = MaxAttempts,
                StartedUtc = DateTime.UtcNow,
                IsSolved = false,
                IsFinished = false,
                Attempts = []
            };
            GuestSessions[GetGuestCacheKey(token)] = state;
            return state;
        }

        private void StoreGuestSession(DailyGameSessionState state)
        {
            GuestSessions[GetGuestCacheKey(state.GuestSessionToken)] = state;
        }

        private void PersistMemberSession(DailyGameSessionState state, DailyCardChallengeDBModel challenge, int memberId, DailyGameAttemptState latestAttempt)
        {
            var dbSession = _sessionRepository.GetByChallengeAndMember(challenge.Id, memberId) ?? throw new InvalidOperationException("Member session not found.");
            dbSession.AttemptsUsed = state.AttemptsUsed;
            dbSession.Status = state.Status;
            dbSession.Solved = state.IsSolved;
            dbSession.FinishedUtc = state.FinishedUtc;
            _sessionRepository.Save(dbSession);

            _guessRepository.Add(new DailyCardGameGuessDBModel
            {
                SessionId = dbSession.Id,
                AttemptNumber = latestAttempt.AttemptNumber,
                GuessedCardId = latestAttempt.GuessedCardId,
                FeedbackJson = SerializeFeedback(latestAttempt.Feedback),
                CreatedUtc = latestAttempt.CreatedUtc
            });
        }

        private static DailyGameSessionState MapSession(DailyCardGameSessionDBModel session, DailyGameAttemptState[] attempts)
        {
            return new DailyGameSessionState
            {
                ChallengeId = session.ChallengeId,
                GuestSessionToken = string.Empty,
                Status = session.Status,
                AttemptsUsed = session.AttemptsUsed,
                MaxAttempts = session.MaxAttempts,
                StartedUtc = session.StartedUtc,
                FinishedUtc = session.FinishedUtc,
                IsSolved = session.Solved,
                IsFinished = session.Status != "InProgress",
                Attempts = attempts
            };
        }

        private DailyGameAttemptState MapAttempt(DailyCardGameGuessDBModel guess, int targetCardId)
        {
            var feedback = DeserializeFeedback(guess.FeedbackJson);
            return new DailyGameAttemptState
            {
                AttemptNumber = guess.AttemptNumber,
                GuessedCardId = guess.GuessedCardId,
                GuessedCardName = _cardService.GetVariant(guess.GuessedCardId)?.DisplayName,
                IsCorrect = guess.GuessedCardId == targetCardId,
                Feedback = feedback,
                CreatedUtc = guess.CreatedUtc
            };
        }

        private static string SerializeFeedback(DailyGameAttributeFeedbackState[] feedback)
        {
            return JsonSerializer.Serialize(feedback);
        }

        private static DailyGameAttributeFeedbackState[] DeserializeFeedback(string json)
        {
            return JsonSerializer.Deserialize<DailyGameAttributeFeedbackState[]>(json) ?? [];
        }

        private static DailyGameAttributeFeedbackState[] BuildFeedback(Card target, Card guess)
        {
            var attributes = new[] { "Cost", "Traits", "Aspects", "Card Type", "Health", "Power", "Rarity", "Location" };

            var differences = attributes.Select(attribute =>
            {
                var targetValues = target.GetMultipleCardAttributeValue(attribute) ?? [];
                var guessValues = guess.GetMultipleCardAttributeValue(attribute) ?? [];
                var matchType = DailyGameRules.CompareValues(targetValues, guessValues);
                return new DailyGameAttributeFeedbackState
                {
                    Name = attribute,
                    MatchType = matchType,
                    GuessValues = guessValues
                };
            }).ToList();
            differences.Add(new DailyGameAttributeFeedbackState
            {
                Name = "Set",
                MatchType = DailyGameRules.CompareValues([target.SetName], [guess.SetName]),
                GuessValues = [guess.SetName]
            });
            return differences.ToArray();
        }

        private static int GetElapsedSeconds(DateTime startedUtc, DateTime finishedUtc)
        {
            return Math.Max(0, (int)(finishedUtc - startedUtc).TotalSeconds);
        }

        private static string GetGuestCacheKey(string token) => $"dailygame_guest_{token}";
    }

    public class DailyGameGuessState
    {
        public required DailyGameSessionState State { get; set; }
        public DailyGameAttemptState? LatestAttempt { get; set; }
        public DailyGameLeaderboardEntryState? CurrentPlacement { get; set; }
    }

    public class DailyGameSessionState
    {
        public int ChallengeId { get; set; }
        public required string GuestSessionToken { get; set; }
        public required string Status { get; set; }
        public int AttemptsUsed { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }
        public bool IsSolved { get; set; }
        public bool IsFinished { get; set; }
        public DailyGameAttemptState[] Attempts { get; set; } = [];
    }

    public class DailyGameAttemptState
    {
        public int AttemptNumber { get; set; }
        public int GuessedCardId { get; set; }
        public string? GuessedCardName { get; set; }
        public bool IsCorrect { get; set; }
        public DailyGameAttributeFeedbackState[] Feedback { get; set; } = [];
        public DateTime CreatedUtc { get; set; }
    }

    public class DailyGameAttributeFeedbackState
    {
        public required string Name { get; set; }
        public required string MatchType { get; set; }
        public string[] GuessValues { get; set; } = [];
    }

    public class DailyGameLeaderboardEntryState
    {
        public int Rank { get; set; }
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }
        public int AttemptsUsed { get; set; }
        public int ElapsedSeconds { get; set; }
        public bool Solved { get; set; }
        public bool IsCurrentPlayer { get; set; }
    }
}
