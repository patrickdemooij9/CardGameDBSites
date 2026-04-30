namespace SkytearHorde.Entities.Models.Business
{
    public class Archetype
    {
        public Guid Id { get; set; }
        public int SiteId { get; set; }
        public int? FormatId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
