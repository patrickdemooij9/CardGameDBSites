namespace CardGameDBSites.API.Models.CardLists
{
    public class CardInListApiModel
    {
        public int ListId { get; set; }
        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public int Amount { get; set; }
    }
}
