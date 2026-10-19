namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class PricingOverviewApiModel
    {
        public required int PriceableCardCount { get; set; }
        public required MissingPriceApiModel[] MissingPrices { get; set; }
        public required TcgPlayerIdConflictApiModel[] Conflicts { get; set; }
        public required PriceSyncStatusApiModel SyncStatus { get; set; }
    }
}
