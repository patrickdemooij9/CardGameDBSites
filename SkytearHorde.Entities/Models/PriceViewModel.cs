namespace SkytearHorde.Entities.Models
{
    public class PriceViewModel
    {
        public double Price { get; set; }
        public string Url { get; set; }

        public PriceViewModel(double price, string url)
        {
            Price = price;
            Url = url;
        }
    }
}
