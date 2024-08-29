using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class VariantTypeViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public bool HasPage { get; set; }
        public string Color { get; set; }
        public string? Initial { get; set; }

        public bool ChildOfBase { get; set; }
        public int? ChildOf { get; set; }

        public VariantTypeViewModel(VariantType variant)
        {
            Id = variant.Id;
            DisplayName = variant.DisplayName;

            HasPage = variant.HasPage;
            Color = variant.Color;
            Initial = variant.Initial;

            ChildOfBase = variant.ChildOfBase;
            ChildOf = variant.ChildOf;
        }
    }
}
