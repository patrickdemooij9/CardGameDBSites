namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameAttemptApiModel
    {
        public int AttemptNumber { get; set; }
        public int GuessedCardId { get; set; }
        public string? GuessedCardName { get; set; }
        public bool IsCorrect { get; set; }
        public DailyGameAttributeFeedbackApiModel[] Feedback { get; set; } = [];
        public DateTime CreatedUtc { get; set; }
    }
}
