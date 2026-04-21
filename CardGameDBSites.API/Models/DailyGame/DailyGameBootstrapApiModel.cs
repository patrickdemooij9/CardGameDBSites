namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameBootstrapApiModel
    {
        public required string GuestSessionToken { get; set; }
        public int MaxAttempts { get; set; }
        public int AttemptsUsed { get; set; }
        public int AttemptsLeft { get; set; }
        public int ElapsedSeconds { get; set; }
        public int BlurLevel { get; set; }
        public bool IsFinished { get; set; }
        public bool IsSolved { get; set; }
        public DailyGameAttemptApiModel[] Attempts { get; set; } = [];
        public DailyGameLeaderboardEntryApiModel[] Leaderboard { get; set; } = [];
        public DailyGameLeaderboardEntryApiModel? CurrentPlacement { get; set; }
    }
}
