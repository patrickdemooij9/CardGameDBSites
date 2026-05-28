namespace CardGameDBSites.API.Models.CardLists
{
    public class CardListApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
