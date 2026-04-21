namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameLeaderboardEntryApiModel
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
