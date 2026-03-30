namespace CardGameDBSites.API.Models.Settings
{
    public class DeckActionApiModel
    {
        public required Guid Id { get; set; }
        public required string DisplayName { get; set; }
        public string Icon { get; set; }
        public required string Type { get; set; }

        public string? PopupTitle { get; set; }
        public string? PopupDescription { get; set; }
        public bool IsCopyClipboard { get; set; }

        public DeckActionApiModel[] SubActions { get; set; } = [];
    }
}
