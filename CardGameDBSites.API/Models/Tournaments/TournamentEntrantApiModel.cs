namespace CardGameDBSites.API.Models.Tournaments
{
    public class TournamentEntrantApiModel
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public int Placement { get; set; }
        public int DeckId { get; set; }
        public string? DeckName { get; set; }
    }
}
