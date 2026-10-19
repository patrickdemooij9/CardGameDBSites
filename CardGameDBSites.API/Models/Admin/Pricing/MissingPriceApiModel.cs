namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class MissingPriceApiModel
    {
        public required PricingCardApiModel Card { get; set; }
        public required MissingPriceReason Reason { get; set; }
    }
}
