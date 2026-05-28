namespace CardGameDBSites.API.Models.CardLists
{
    public class CardListItemApiModel
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int? VariantId { get; set; }
        public int Amount { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
