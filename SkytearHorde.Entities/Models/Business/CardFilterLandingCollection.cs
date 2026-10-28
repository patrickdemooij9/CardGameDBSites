using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.Business
{
    public class CardFilterLandingCollection
    {
        public required CardFilterCollection Content { get; init; }
        public required string Url { get; init; }
        public required string Attribute { get; init; }
        public required CardFilterLandingItem[] Landings { get; init; }
    }
}
