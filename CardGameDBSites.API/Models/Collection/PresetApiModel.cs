namespace CardGameDBSites.API.Models.Collection
{
    public class PresetApiModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        // TODO: Add image once backend support is available
        public int CardCount { get; set; }
    }
}
