namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class PricingCardApiModel
    {
        public required int BaseId { get; set; }
        public required int VariantId { get; set; }
        public required string DisplayName { get; set; }
        public required int SetId { get; set; }
        public required string SetName { get; set; }
        public required string VariantTypeName { get; set; }
        public string? TcgPlayerId { get; set; }
    }
}
