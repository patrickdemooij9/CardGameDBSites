namespace SkytearHorde.Entities.Models.Business
{
    public interface IComment
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
    }
}
