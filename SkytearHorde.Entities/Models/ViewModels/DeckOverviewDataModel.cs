using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DeckOverviewDataModel
    {
        public PagedResult<Deck> Decks { get; set; }
        public int DecksPerRow { get; set; }
        public int Page { get; set; }
        public string BaseUrl { get; set; }

        public DeckOverviewDataModel(PagedResult<Deck> decks)
        {
            Decks = decks;
        }
    }
}
