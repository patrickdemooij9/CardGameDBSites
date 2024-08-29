namespace SkytearHorde.Entities.Models.Business
{
    public class CollectionCardItemGrouped
    {
        public int CardId { get; set; }
        public Dictionary<int, int> VariantAmounts { get; set; }

        public CollectionCardItemGrouped(int cardId)
        {
            CardId = cardId;

            VariantAmounts = [];
        }
    }
}
