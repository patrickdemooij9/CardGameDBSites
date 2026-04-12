namespace CardGameDBSites.API.Models.Community
{
    public class CommunityBlogPostApiModel
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public string? ImageUrl { get; set; }
        public required string Url { get; set; }
        public DateTime PublishedDate { get; set; }
        public required string Author { get; set; }
        public required string TagType { get; set; }
    }
}
