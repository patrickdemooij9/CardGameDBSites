namespace SkytearHorde.Entities.Models.Business
{
    public class Period
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int FormatId { get; set; }
        public string Name { get; set; }
        public DateTime StartingDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
    }
}
