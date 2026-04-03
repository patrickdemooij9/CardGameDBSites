namespace CardGameDBSites.API.Models.Comments
{
    public class CreateCommentPostModel
    {
        public int DeckId { get; set; }
        public int CardId { get; set; }
        public string Comment { get; set; }
    }
}
