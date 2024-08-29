namespace SkytearHorde.Entities.Models.Business
{
    public class DeckComment : IComment
    {
        public int Id { get; set; }
        public int DeckId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedBy { get; set; }
        public int SiteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
    }
}
