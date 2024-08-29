namespace SkytearHorde.Entities.Models.Business
{
    public class CardPrice
    {
        public int? VariantId { get; set; }
        public double MainPrice { get; set; }
        public double LowestPrice { get; set; }
        public double HighestPrice { get; set; }
        public DateTime DateUtc { get; set; }
    }
}
