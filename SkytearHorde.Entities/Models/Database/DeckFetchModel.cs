namespace SkytearHorde.Entities.Models.Database
{
    public class DeckFetchModel
    {
        public int Id { get; set; }
        public int LatestVersionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public bool Published { get; set; }
        public int SiteId { get; set; }
        public int DeckType { get; set; }
        public bool IsLegal { get; set; }
        public int Score { get; set; }
        public int AmountOfLikes { get; set; }
        public double CollectionCompare { get; set; }
    }
}
