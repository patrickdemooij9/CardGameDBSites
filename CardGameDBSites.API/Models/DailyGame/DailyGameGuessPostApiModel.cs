namespace CardGameDBSites.API.Models.DailyGame
{
    public class DailyGameGuessPostApiModel
    {
        public int GuessedCardId { get; set; }
        public string? GuestSessionToken { get; set; }
    }
}
