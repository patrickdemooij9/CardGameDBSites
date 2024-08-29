using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Models.Business
{
    public class CardPriceGroup : IEntity
    {
        public int Id => CardId;

        public int SourceId { get; set; }
        public int CardId { get; set; }
        public List<CardPrice> Prices { get; set; }

        public CardPriceGroup()
        {
            Prices = [];
        }

        public double GetLowest()
        {
            return Prices.Min(it => it.MainPrice);
        }

        public CardPrice? GetByVariant(int variantId)
        {
            return Prices.FirstOrDefault(it => it.VariantId == variantId);
        }
    }
}
