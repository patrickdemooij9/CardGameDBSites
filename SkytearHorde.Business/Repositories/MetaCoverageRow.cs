namespace SkytearHorde.Business.Repositories
{
    public class MetaCoverageRow
    {
        public int EventCount { get; set; }
        public int EntrantCount { get; set; }
        public int EntrantsWithDeck { get; set; }
        public DateTime? FirstEventUtc { get; set; }
        public DateTime? LastEventUtc { get; set; }
    }
}
