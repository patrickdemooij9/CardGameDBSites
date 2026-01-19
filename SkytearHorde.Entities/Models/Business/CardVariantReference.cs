namespace SkytearHorde.Entities.Models.Business
{
    public class CardVariantReference
    {
        public int? VariantTypeId { get; set; }
        public int CardVariantId { get; set; }
        public int SetId { get; set; }

        public CardVariantReference(int? variantTypeId, int cardVariantId, int setId)
        {
            VariantTypeId = variantTypeId;
            CardVariantId = cardVariantId;
            SetId = setId;
        }
    }
}
