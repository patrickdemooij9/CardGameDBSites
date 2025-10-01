namespace CardGameDBSites.API.Models.Settings
{
    public class DeckActionApiModel
    {
        public required Guid Id { get; set; }
        public required string DisplayName { get; set; }
        public required string Icon { get; set; }
        public required string Type { get; set; }
    }
}
