namespace CardGameDBSites.API.Models.CardLists
{
    public class AddCardListItemPostModel
    {
        public int CardId { get; set; }
        public int? VariantId { get; set; }
        public int Amount { get; set; } = 1;
    }
}
