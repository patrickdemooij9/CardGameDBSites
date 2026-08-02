namespace CardGameDBSites.API.Models.Meta
{
    /// <summary>
    /// Persisted usage/winrate for a single card in a period, overlaid onto the leader cards on the
    /// MetaCardOverview page. A card with no snapshot data reports zeros.
    /// </summary>
    public class MetaCardStatApiModel
    {
        public int CardId { get; set; }
        public int DeckCount { get; set; }
        public double UsagePercentage { get; set; }
        public double WinratePercentage { get; set; }
    }
}
