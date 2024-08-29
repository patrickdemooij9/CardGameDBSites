using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardVariantViewModel
    {
        public int Id { get; set; }
        public int? TypeId { get; set; }

        public CardVariantViewModel(Card card)
        {
            Id = card.VariantId;
            TypeId = card.VariantTypeId;
        }
    }
}
