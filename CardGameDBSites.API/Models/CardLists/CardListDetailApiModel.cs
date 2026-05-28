namespace CardGameDBSites.API.Models.CardLists
{
    public class CardListDetailApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? OwnerName { get; set; }
        public CardListItemApiModel[] Items { get; set; } = Array.Empty<CardListItemApiModel>();
    }
}
