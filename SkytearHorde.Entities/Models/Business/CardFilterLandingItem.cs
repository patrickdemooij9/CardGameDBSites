namespace SkytearHorde.Entities.Models.Business
{
    public class CardFilterLandingItem
    {
        public required string Name { get; init; }
        public required string Url { get; init; }
        public required string Value { get; init; }
        public int CardCount { get; init; }
        public bool IsGenerated { get; init; }
    }
}
