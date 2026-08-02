namespace CardGameDBSites.API.Models.Meta
{
    /// <summary>
    /// The meta detail page URL for a card. <see cref="Url"/> is null when the card isn't a leader
    /// (so has no meta page).
    /// </summary>
    public class MetaCardLinkApiModel
    {
        public string? Url { get; set; }
    }
}
