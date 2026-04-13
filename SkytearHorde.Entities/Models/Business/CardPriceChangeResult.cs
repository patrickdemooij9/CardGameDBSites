namespace SkytearHorde.Entities.Models.Business
{
    public class CardPriceChangeResult
    {
        public int CardId { get; set; }
        public int? VariantId { get; set; }
        public double CurrentPrice { get; set; }
        public double PreviousPrice { get; set; }
    }
}
