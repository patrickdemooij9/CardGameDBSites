namespace CardGameDBSites.API.Models.Cards
{
    public class CardVariantReferenceApiModel
    {
        public int? VariantTypeId { get; set; }
        public int CardVariantId { get; set; }
        public int SetId { get; set; }

        public CardVariantReferenceApiModel(int? variantTypeId, int cardVariantId, int setId)
        {
            VariantTypeId = variantTypeId;
            CardVariantId = cardVariantId;
            SetId = setId;
        }
    }
}
