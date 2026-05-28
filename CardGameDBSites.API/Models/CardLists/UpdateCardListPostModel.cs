namespace CardGameDBSites.API.Models.CardLists
{
    public class UpdateCardListPostModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
    }
}
