namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchFilter
    {
        public required string Alias { get; set; }
        public string[] Values { get; set; }
        public CardSearchFilterMode Mode { get; set; }

        public CardSearchFilter()
        {
            Values = [];
            Mode = CardSearchFilterMode.Contains;
        }
    }
}
