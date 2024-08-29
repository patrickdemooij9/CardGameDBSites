namespace SkytearHorde.Entities.Models.Business
{
    public class ContentCreatorBlogPost
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public string? ImageUrl { get; set; }
        public required string Url { get; set; }
        public required DateTime PublishedDate { get; set; }
        public required int CreatorId { get; set; }
    }
}
