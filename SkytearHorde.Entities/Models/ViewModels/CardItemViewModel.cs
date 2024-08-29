using Umbraco.Cms.Core.Models;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardItemViewModel
    {
        public int BaseId { get; set; }
        public int SetId { get; set; }
        public string SetCode { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public MediaWithCrops Image { get; set; }
        public CardAbilityViewModel[] Abilities { get; set; }
        public PriceViewModel? Price { get; set; }
        public CardVariantViewModel[] Variants { get; set; }
        public CardCollectionViewModel? Collection { get; set; }

        public CardItemViewModel(int baseId)
        {
            BaseId = baseId;

            Abilities = [];
            Variants = [];
        }

        public CardAbilityViewModel? GetAbilityByType(string type)
        {
            return Abilities.FirstOrDefault(it => it.Type.Equals(type));
        }

        public CardVariantViewModel GetMainVariant()
        {
            return Variants.FirstOrDefault(it => it.TypeId is null) ?? Variants.First();
        }

        public CardVariantViewModel? GetVariant(int? typeId)
        {
            return Variants.FirstOrDefault(it => it.TypeId == typeId);
        }
    }
}
