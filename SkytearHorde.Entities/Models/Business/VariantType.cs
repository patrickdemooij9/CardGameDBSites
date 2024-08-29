using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.Business
{
    public class VariantType
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public bool HasPage { get; set; }
        public bool RequiresPage { get; set; }
        public string Color { get; set; }
        public string? Initial { get; set; }
        public string? Identifier { get; set; }

        public int? ChildOf { get; set; }
        public bool ChildOfBase { get; set; }

        public ISquadRequirementConfig[] Requirements { get; }

        public VariantType(Variant variant)
        {
            Id = variant.InternalID;
            DisplayName = variant.DisplayName!;

            HasPage = variant.HasPage;
            RequiresPage = variant.RequiresPage;
            Color = variant.Color!;
            Initial = variant.Initial;
            Identifier = variant.Identifier?.Name;

            ChildOf = (variant.ChildOf as Variant)?.InternalID;
            ChildOfBase = variant.ChildOfBase;

            Requirements = variant.Requirements?.Select(it => (ISquadRequirementConfig) it.Content).ToArray() ?? [];
        }
    }
}
