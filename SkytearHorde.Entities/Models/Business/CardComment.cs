namespace SkytearHorde.Entities.Models.Business
{
    public class CardComment : IComment
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedBy { get; set; }
        public int SiteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
    }
}
