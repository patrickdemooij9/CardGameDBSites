namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class TcgPlayerIdConflictApiModel
    {
        public required string TcgPlayerId { get; set; }
        public required PricingCardApiModel[] Cards { get; set; }
    }
}
