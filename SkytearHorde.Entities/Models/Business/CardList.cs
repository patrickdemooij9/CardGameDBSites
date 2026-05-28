namespace SkytearHorde.Entities.Models.Business
{
    public class CardList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CardListItem> Items { get; set; }

        public CardList(string name, int createdBy)
        {
            Name = name;
            CreatedBy = createdBy;
            IsPublic = false;
            Items = new List<CardListItem>();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
