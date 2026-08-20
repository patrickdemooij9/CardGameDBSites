namespace SkytearHorde.Business.Repositories
{
    public class MetaCardStatRow
    {
        public int CardId { get; set; }
        public int DeckCount { get; set; }
        public int EventCount { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Top8Count { get; set; }
        public int FirstPlaceCount { get; set; }
    }
}
