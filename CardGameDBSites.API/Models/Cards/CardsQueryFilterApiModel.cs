using SkytearHorde.Business.Services.Search;

namespace CardGameDBSites.API.Models.Cards
{
    public class CardsQueryFilterApiModel
    {
        public required string Alias { get; set; }
        public string[] Values { get; set; }
        public CardSearchFilterMode Mode { get; set; }

        public CardsQueryFilterApiModel()
        {
            Values = [];
            Mode = CardSearchFilterMode.Contains;
        }
    }
}
