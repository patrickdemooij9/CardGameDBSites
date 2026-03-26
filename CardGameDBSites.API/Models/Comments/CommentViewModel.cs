namespace CardGameDBSites.API.Models.Comments
{
    public class CommentViewModel
    {
        public required int Id { get; set; }
        public required string Comment { get; set; }
        public required int CreatedById { get; set; }
        public required string CreatedByName { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
