namespace SkytearHorde.Entities.Models.Business
{
    public class DeckFolder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SiteId { get; set; }

        public DeckFolder(string name, int createdBy, int siteId)
        {
            Name = name;
            CreatedBy = createdBy;
            SiteId = siteId;
            CreatedDate = DateTime.UtcNow;
        }
    }
}
