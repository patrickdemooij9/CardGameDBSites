namespace SkytearHorde.Entities.Models.Business
{
    public class DeckList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<int> DeckIds { get; set; }

        public DeckList(string name, int createdBy)
        {
            Name = name;
            CreatedBy = createdBy;
            DeckIds = new List<int>();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
