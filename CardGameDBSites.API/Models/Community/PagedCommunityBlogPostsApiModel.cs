namespace CardGameDBSites.API.Models.Community
{
    public class PagedCommunityBlogPostsApiModel
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public required CommunityBlogPostApiModel[] Items { get; set; }
    }
}
