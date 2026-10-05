namespace CardGameDBSites.API.Models.Collection
{
    public class AddCollectionCardsApiModel
    {
        public int CardId { get; set; }
        public Dictionary<int, int> Values { get; set; } = new();
    }
}
