namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardCollectionViewModel
    {
        public Dictionary<int, int> Amounts { get; set; }

        public CardCollectionViewModel()
        {
            Amounts = [];
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
            return Amounts.ContainsKey(variantId);
        }
    }
}
