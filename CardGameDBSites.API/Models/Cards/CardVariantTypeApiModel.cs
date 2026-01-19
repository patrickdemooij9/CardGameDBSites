namespace CardGameDBSites.API.Models.Cards
{
    public class CardVariantTypeApiModel
    {
        public required int Id { get; set; }
        public required string DisplayName { get; set; }

        public string? Color { get; set; }
        public string? Initial { get; set; }
    }
}
