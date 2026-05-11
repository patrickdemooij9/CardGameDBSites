namespace CardGameDBSites.API.Models.Settings
{
    public class SquadSettingsOptionApiModel
    {
        public Guid Id { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
