namespace SkytearHorde.Entities.Models.Business
{
    public class CardListItem
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int CardId { get; set; }
        public int? VariantId { get; set; }
        public int Amount { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
