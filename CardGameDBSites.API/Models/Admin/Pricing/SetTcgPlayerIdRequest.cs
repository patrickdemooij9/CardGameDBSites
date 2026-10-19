namespace CardGameDBSites.API.Models.Admin.Pricing
{
    public class SetTcgPlayerIdRequest
    {
        public required int VariantId { get; set; }
        public string? TcgPlayerId { get; set; }
    }
}
