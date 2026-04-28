namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameAttributeFeedbackApiModel
    {
        public required string Name { get; set; }
        public required string MatchType { get; set; }
        public string[] GuessValues { get; set; } = [];
    }
}
