namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CollectionCardItemViewModel
    {
        public CardItemViewModel Card { get; set; }
        public Dictionary<int, int> Amounts { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }

        public CollectionCardItemViewModel(CardItemViewModel card)
        {
            Card = card;
            Amounts = [];
            VariantTypes = [];
        }

        public int GetTotalAmount()
        {
            return Amounts.Values.Sum();
        }

        public int GetAmount(int variantId)
        {
            if (Amounts.TryGetValue(variantId, out var amount))
            {
                return amount;
            }
            return 0;
        }

        public bool HasVariant(int variantId)
        {
            return Card.Variants.Any(it => it.Id == variantId);
        }
    }
}
